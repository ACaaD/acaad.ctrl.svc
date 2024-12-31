using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;

using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.CliOutputParser.Errors;
using Oma.WndwCtrl.FpCore.TransformerStacks.Flow;

using static Oma.WndwCtrl.FpCore.TransformerStacks.Flow.FlowExtensions;

using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class DelegatingTransformer : IRootTransformer
{
    private readonly ILogger<DelegatingTransformer> _logger;
    private readonly IEnumerable<IOutcomeTransformer> _transformers;
    
    [ExcludeFromCodeCoverage]
    public bool Handles(ITransformation transformation) => true;

    public DelegatingTransformer(ILogger<DelegatingTransformer> logger, IEnumerable<IOutcomeTransformer> transformers)
    {
        _logger = logger;
        _transformers = transformers;
    }
    
    public static FlowT<TransformationState, TransformationOutcome> OverallFlow => 
    (
        // from tuple in Flow<TransformationState>.asks2(state => (state.Command, state.OutcomeTransformers))
        from outcome in initialOutcome
        from allT in transformations
        
        // TODO: Presumably the order is incorrect here and the fold must be applied first, then the lift.
        from eitherChained in 
            allT.Fold<Seq, ITransformation, FlowT<TransformationState, TransformationOutcome>>(outcome, t =>
            {
                return lastEither =>
                {
                    FlowT<TransformationState, TransformationOutcome> res = 
                        from transformer in FindApplicableTransformer(t)
                        from result in ExecuteTransformerIO(t, transformer, lastEither)
                        select result;

                    return res;
                };
            })
        
        select eitherChained
    ).As();

    public static FlowT<TransformationState, IOutcomeTransformer> FindApplicableTransformer(
        ITransformation transformation
    ) => (
        from _ in Flow<TransformationState>.asks2(state => state.Command)
        
        from allTransformers in config.Map(cfg => cfg.OutcomeTransformers)
        
        from found in Flow<TransformationState>.lift(
           allTransformers.Find(t => t.Handles(transformation))
               .ToEither<FlowError>(() => FlowError.NoTransformerFound(transformation))
        )
        
        select found
    ).As();
    
    public static FlowT<TransformationState, TransformationState> config =>
        new (ReaderT.ask<EitherT<FlowError, IO>, TransformationState>());
    
    private static FlowT<TransformationState, Seq<ITransformation>> transformations =>
        config.Map(cfg => cfg.Command)
            .Map(command => command.Transformations)
            .Map(t => new Seq<ITransformation>(t))
            .As(); // TODO: Figure out why this is needed here.

    private static FlowT<TransformationState, Either<FlowError, TransformationOutcome>> initialOutcome =>
        config.Map(cfg => cfg.InitialOutcome).As();
    
    // TODO: Is passing the FlowT here correct?
    private static FlowT<TransformationState, TransformationOutcome> ExecuteTransformerIO(
        ITransformation transformation, IOutcomeTransformer transformer, FlowT<TransformationState, TransformationOutcome> outcome
    ) =>
    (
        from unwrapped in outcome
    
        /* IO COMPUTATION */
        from ioRes in Flow<TransformationState>.liftAsync(async envIO => 
            await transformer.TransformCommandOutcomeAsync(transformation, unwrapped, envIO.Token)
        )
        /* IO COMPUTATION */
        
        /* LIFT INTO EITHER */
        from result in Flow<TransformationState>.lift(ioRes)
        /* LIFT INTO EITHER */
        
        select result
    ).As();
    
    public async Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        ICommand command,
        Either<FlowError, CommandOutcome> commandOutcome, 
        CancellationToken cancelToken = default
    )
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(commandOutcome);
        _logger.LogTrace("Received command outcome to transform.");
        
        TransformationState initialState = new(_logger, _transformers, command, commandOutcome);
        
        // No point doing this here
        Expression<Func<TransformationState, EnvIO, ValueTask<Either<FlowError, TransformationOutcome>>>> expression = (config, io) 
            => OverallFlow.ExecuteFlow
                .Run(config)
                .Run()
                .RunAsync(io);

        var func = expression.Compile();

        EnvIO envIO = EnvIO.New(token: cancelToken);
        
        var outcomeWithState = await func(initialState, envIO);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);

        return outcomeWithState;
    }
}