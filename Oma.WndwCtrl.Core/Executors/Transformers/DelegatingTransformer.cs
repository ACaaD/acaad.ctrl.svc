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
    
    public static FlowT<TransformationState, TransformationOutcome> OverallFlowT => (
        from tuple in FindMatchingTransformer
        from res in ExecuteTransformerIO(tuple)
        select res
    ).As();
    
    private static FlowT<TransformationState, (ITransformation, IOutcomeTransformer)> FindMatchingTransformer =>
    (
        from tuple in Flow<TransformationState>.asks2(state => (state.Command, state.OutcomeTransformers))
        
        // TODO: Handle in "loop" somehow
        from transformation in Flow<TransformationState>.lift<ITransformation>(NextTransformation(tuple.Command)
            .ToEither<FlowError>(() => new FlowError($"Could not locate current transformation", isExpected: false, isExceptional: true)))
        
        from transformer in Flow<TransformationState>.lift(tuple.OutcomeTransformers.Find(t => t.Handles(transformation))
            .ToEither<FlowError>(() => new FlowError($"Could not locate transformer for input '{transformation}'", isExceptional: true)))
        
        select (transformation, transformer)
    ).As();

    private static FlowT<TransformationState, TransformationOutcome> ExecuteTransformerIO(
        (ITransformation transformation, IOutcomeTransformer transformer) tuple
    ) =>
    (
        from _ in Flow<TransformationState>.asks2(state => state.Command)
        
        /* IO COMPUTATION */
        from either in Flow<TransformationState>.liftAsync(async envIO =>
        {
            Either<FlowError, TransformationOutcome> currentValue = default; // TODO

            Either<FlowError, TransformationOutcome> tRes =
                await tuple.transformer.TransformCommandOutcomeAsync(tuple.transformation, currentValue, envIO.Token);
            
            return tRes;
        })
        /* IO COMPUTATION */
        
        /* LIFT INTO EITHER */
        from result in Flow<TransformationState>.lift(either)
        /* LIFT INTO EITHER */
        
        select result
    ).As();
    
    private static FlowT<TransformationState, TransformationOutcome> ExecuteTransformerSync((ITransformation transformation, IOutcomeTransformer transformer) tuple) =>
    (
        from currentVal in Flow<TransformationState>.lift((Either<FlowError, TransformationOutcome>)default)
        from _ in Flow<TransformationState>.lift(tuple.transformer.TransformCommandOutcomeAsync(tuple.transformation, currentVal).GetAwaiter().GetResult())
        select _
    ).As();
    
    private static Option<ITransformation> NextTransformation(ICommand command)
    {
        var result = command.Transformations.FirstOrDefault();

        if (result is not null)
        {
            return Option<ITransformation>.Some(result);
        }
        
        return Option<ITransformation>.None;
    }
    
    public async Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        ICommand command,
        Either<FlowError, CommandOutcome> commandOutcome, 
        CancellationToken cancelToken = default
    )
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(commandOutcome);
        _logger.LogTrace("Received command outcome to transform.");
        
        TransformationState initialState = new(_logger, _transformers, command);
        
        // No point doing this here
        Expression<Func<TransformationState, EnvIO, ValueTask<Either<FlowError, TransformationOutcome>>>> expression = (config, io) 
            => OverallFlowT.ExecuteFlow
                .Run(config)
                .Run()
                .RunAsync(io);

        var func = expression.Compile();

        EnvIO envIO = EnvIO.New(token: cancelToken);
        
        var outcomeWithState = await func(initialState, envIO);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);

        return outcomeWithState;
    }
    
    // private static MyState<TransformationState, TransformationOutcome> ExecuteTransformersAsync(TransformationOutcome transformationOutcome)
    // {
    //     var applyTransformationCallChain = FindTransformer()
    //         .BindAsync(LogTransformerExecution)
    //         .BindAsync(ExecuteTransformerAsync)
    //         .BindAsync(StoreTransformationOutcomeAsync);
    //     
    //     return async state =>
    //     {
    //         Either<FlowError, (TransformationState State, TransformationOutcome Outcome)> currentState 
    //             = (state with { CurrentOutcome = transformationOutcome }, transformationOutcome);
    //         
    //         foreach (ITransformation transformation in state.Command.Transformations)
    //         {
    //             currentState = await currentState.BindAsync(tuple =>
    //             {
    //                 TransformationState actualState = tuple.State with
    //                 {
    //                     CurrentTransformation = transformation,
    //                 };
    //                 
    //                 return applyTransformationCallChain.RunAsync(actualState);
    //             });
    //         }
    //         
    //         return currentState;
    //     };
    // }
    //
    // private static MyState<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)> FindTransformer()
    // {
    //     return state =>
    //     {
    //         /* This sucks... */
    //         if (state.CurrentTransformation is null)
    //         {
    //             return Fail<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)>(new ProgrammingError(
    //                 $"Current transformation is null. This should never happen.",
    //                 50_000));
    //         }
    //         
    //         if (state.CurrentOutcome is null)
    //         {
    //             return Fail<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)>(new ProgrammingError(
    //                 $"Current outcome is null. This should never happen.",
    //                 50_000));
    //         }
    //         /* This sucks... */
    //         
    //         IOutcomeTransformer? transformer = state.OutcomeTransformers
    //             .FirstOrDefault(executor => executor.Handles(state.CurrentTransformation));
    //
    //         return transformer is null
    //             ? Fail<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)>(new ProgrammingError(
    //                 $"No transformation executor found that handles transformation type {state.CurrentTransformation.GetType().FullName}.",
    //                 2))
    //             : Success(state, (transformer, InitialOutcome: state.CurrentOutcome));
    //     };
    // }
    //
    // private static MyState<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)> LogTransformerExecution(
    //     (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome) tuple
    // )
    // {
    //     return state =>
    //     {
    //         state.Logger.LogInformation("Executing transformer {type}.", tuple.Transformer.GetType().Name);
    //         return Success(state, tuple);
    //     };
    // }
    //
    // private static MyState<TransformationState, TransformationOutcome> ExecuteTransformerAsync(
    //     (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome) tuple
    // )
    // {
    //     return async state =>
    //     {
    //         if (state.CurrentTransformation is null)
    //         {
    //             return Left<FlowError>(new ProgrammingError("Current transformation is null when executing (sub) transformer. This should never happen.", 50_001));
    //         }
    //         
    //         Either<FlowError, TransformationOutcome> transformerInput = Right(tuple.TransformationOutcome);
    //
    //         var res = await tuple.Transformer
    //             .TransformCommandOutcomeAsync(state.CurrentTransformation, transformerInput);
    //
    //         return res.BiBind<FlowError, (TransformationState, TransformationOutcome)>(
    //             left => left,
    //             right => (state, right)
    //         );
    //     };
    // }
    //
    // private static MyState<TransformationState, TransformationOutcome> StoreTransformationOutcomeAsync(
    //     TransformationOutcome transformationOutcome
    // )
    // {
    //     return state => Success(state with { CurrentOutcome = transformationOutcome }, transformationOutcome);
    // }
}