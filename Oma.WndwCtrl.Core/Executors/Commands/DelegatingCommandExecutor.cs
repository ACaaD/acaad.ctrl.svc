using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.TypeClasses;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public class DelegatingCommandExecutor : ICommandExecutor
{
    private readonly ILogger<DelegatingCommandExecutor> _logger;
    private readonly IEnumerable<ICommandExecutor> _commandExecutors;

    private readonly MyState<CommandState, CommandOutcome> _callChain;
    
    [ExcludeFromCodeCoverage]
    public bool Handles(ICommand command) => true;
    
    public DelegatingCommandExecutor(
        ILogger<DelegatingCommandExecutor> logger,
        IEnumerable<ICommandExecutor> commandExecutors
    )
    {
        _logger = logger;
        _commandExecutors = commandExecutors;

        _callChain = SetStartTime()
            .BindAsync(FindCommandExecutor)
            .BindAsync(RunExecutor);
    }
    
    public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default)
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(command);
        _logger.LogTrace("Received command to execute.");

        CommandState initialState = new(_logger, _commandExecutors, command);
        
        var outcomeWithState = await _callChain.RunAsync(initialState);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);
        
        return outcomeWithState.BiBind<CommandOutcome>( 
            tuple => tuple.Outcome with { ExecutionDuration = Option<TimeSpan>.Some(swExec.Elapsed) }, 
            err => err
        );
    }

    private static MyState<CommandState, Unit> SetStartTime()
    {
        return state => Task.FromResult<Either<FlowError, (CommandState State, Unit Outcome)>>((state with { StartDate = DateTime.UtcNow }, Unit.Default));
    }
    
    private static MyState<CommandState, ICommandExecutor> FindCommandExecutor(Unit _)
    {
        return state =>
        {
            ICommandExecutor? executor = state.CommandExecutors.FirstOrDefault(executor => executor.Handles(state.Command));
            
            if (executor is null)
            {
                state.Logger.LogError("No command executor found that handles command type {typeName}.", state.Command.GetType().FullName);

                return Task.FromResult<Either<FlowError, (CommandState State, ICommandExecutor Outcome)>>(Prelude.Left<FlowError>(new ProgrammingError(
                    $"No command executor found that handles command type {state.Command.GetType().FullName}.",
                    2)));
            }
        
            return Task.FromResult<Either<FlowError, (CommandState State, ICommandExecutor Outcome)>>((state, executor));
        };
    }
    
    private static MyState<CommandState, CommandOutcome> RunExecutor(ICommandExecutor commandExecutor)
    {
        return async state =>
        {
            state.Logger.LogDebug("Executing command {CommandName} with {ExecutedRetries} retries.", state.Command.GetType().Name, state.ExecutedRetries);
            
            var either = await commandExecutor.ExecuteAsync(state.Command);

            return either.BiBind<(CommandState, CommandOutcome)>(
                Right: outcome => Prelude.Right((state, outcome)),
                Left: err => err
            );
        };
    }
}