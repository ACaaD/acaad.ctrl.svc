using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.CliOutputParser.Errors;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.Executors.Transformers;

public class DelegatingTransformer : IRootTransformer
{
    private readonly ILogger<DelegatingTransformer> _logger;
    private readonly IEnumerable<IOutcomeTransformer> _transformers;

    private readonly MyState<TransformationState, TransformationOutcome> _callChain;

    [ExcludeFromCodeCoverage]
    public bool Handles(ITransformation transformation) => true;

    public DelegatingTransformer(ILogger<DelegatingTransformer> logger, IEnumerable<IOutcomeTransformer> transformers)
    {
        _logger = logger;
        _transformers = transformers;

        _callChain = ApplyCommandToTransformationState()
            .BindAsync(ExecuteTransformersAsync);
    }
    
    private static Task<Either<FlowError, (TState, TOutcome)>> Success<TState, TOutcome>(TState state, TOutcome outcome) 
        => Task.FromResult<Either<FlowError, (TState, TOutcome)>>(Right((state, outcome)));
    
    private static Task<Either<FlowError, (TState, TOutcome)>> Fail<TState, TOutcome>(FlowError error) 
        => Task.FromResult<Either<FlowError, (TState, TOutcome)>>(Left(error));
    
    public async Task<Either<FlowError, TransformationOutcome>> TransformCommandOutcomeAsync(
        ICommand command,
        Either<FlowError, CommandOutcome> commandOutcome, 
        CancellationToken cancelToken = default
    )
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(commandOutcome);
        _logger.LogTrace("Received command outcome to transform.");
        
        TransformationState initialState = new(_logger, _transformers, command, commandOutcome, cancelToken);
        
        var outcomeWithState = await _callChain.RunAsync(initialState);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);
        
        return outcomeWithState.BiBind<TransformationOutcome>( 
            tuple => tuple.Outcome, 
            err => err
        );
    }

    private static MyState<TransformationState, TransformationOutcome> ApplyCommandToTransformationState()
    {
        return state =>
        {
            var result = state.CommandExecutionOutcome.BiBind<(TransformationState State, TransformationOutcome Outcome)>(
                Right: outcome => (state with { StartDate = DateTime.UtcNow } , new(outcome)),
                Left: err => err
            );

            return Task.FromResult(result);
        };
    }
    
    private static MyState<TransformationState, TransformationOutcome> ExecuteTransformersAsync(TransformationOutcome transformationOutcome)
    {
        var applyTransformationCallChain = FindTransformer()
            .BindAsync(LogTransformerExecution)
            .BindAsync(ExecuteTransformerAsync)
            .BindAsync(StoreTransformationOutcomeAsync);
        
        return async state =>
        {
            Either<FlowError, (TransformationState State, TransformationOutcome Outcome)> currentState 
                = (state with { CurrentOutcome = transformationOutcome }, transformationOutcome);
            
            foreach (ITransformation transformation in state.Command.Transformations)
            {
                currentState = await currentState.BindAsync(tuple =>
                {
                    TransformationState actualState = tuple.State with
                    {
                        CurrentTransformation = transformation,
                    };
                    
                    return applyTransformationCallChain.RunAsync(actualState);
                });
            }
            
            return currentState;
        };
    }
    
    private static MyState<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)> FindTransformer()
    {
        return state =>
        {
            /* This sucks... */
            if (state.CurrentTransformation is null)
            {
                return Fail<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)>(new ProgrammingError(
                    $"Current transformation is null. This should never happen.",
                    50_000));
            }
            
            if (state.CurrentOutcome is null)
            {
                return Fail<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)>(new ProgrammingError(
                    $"Current outcome is null. This should never happen.",
                    50_000));
            }
            /* This sucks... */
            
            IOutcomeTransformer? transformer = state.OutcomeTransformers
                .FirstOrDefault(executor => executor.Handles(state.CurrentTransformation));

            return transformer is null
                ? Fail<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)>(new ProgrammingError(
                    $"No transformation executor found that handles transformation type {state.CurrentTransformation.GetType().FullName}.",
                    2))
                : Success(state, (transformer, InitialOutcome: state.CurrentOutcome));
        };
    }

    private static MyState<TransformationState, (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome)> LogTransformerExecution(
        (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome) tuple
    )
    {
        return state =>
        {
            state.Logger.LogInformation("Executing transformer {type}.", tuple.Transformer.GetType().Name);
            return Success(state, tuple);
        };
    }

    private static MyState<TransformationState, TransformationOutcome> ExecuteTransformerAsync(
        (IOutcomeTransformer Transformer, TransformationOutcome TransformationOutcome) tuple
    )
    {
        return async state =>
        {
            if (state.CurrentTransformation is null)
            {
                return Left<FlowError>(new ProgrammingError("Current transformation is null when executing (sub) transformer. This should never happen.", 50_001));
            }
            
            Either<FlowError, TransformationOutcome> transformerInput = Right(tuple.TransformationOutcome);

            var res = await tuple.Transformer
                .TransformCommandOutcomeAsync(state.CurrentTransformation, transformerInput);

            return res.BiBind<(TransformationState, TransformationOutcome)>(
                right => (state, right),
                left => left
            );
        };
    }

    private static MyState<TransformationState, TransformationOutcome> StoreTransformationOutcomeAsync(
        TransformationOutcome transformationOutcome
    )
    {
        return state => Success(state with { CurrentOutcome = transformationOutcome }, transformationOutcome);
    }
}