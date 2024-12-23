using System.Diagnostics;
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

    public bool Handles(ICommand command) => true;
    
    public DelegatingCommandExecutor(
        ILogger<DelegatingCommandExecutor> logger,
        IEnumerable<ICommandExecutor> commandExecutors
    )
    {
        _logger = logger;
        _commandExecutors = commandExecutors;
    }
    
    // public delegate Either<CommandError, (S, A)> State<S, A>(S state);
    
    public async Task<MyState<CommandState, CommandOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default)
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(command);
        _logger.LogTrace("Received command to execute.");

        CommandState initialState = new CommandState(command);

        var outcomeWithState = await FindCommandExecutorWithState()
            .BindAsync(RunExecutorWithState)
            .RunAsync(initialState)
            .AsTask();
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);
        
        // return outcome.BiBind<CommandOutcome>(
        //     commandOutcome => commandOutcome with
        //     {   
        //         ExecutionDuration = swExec.Elapsed
        //     },
        //     error => error with
        //     {   
        //         ExecutionDuration = swExec.Elapsed
        //     });

        return default;
    }
    
    private Abstractions.MyState<CommandState, ICommandExecutor> FindCommandExecutorWithState()
    {
        return async state =>
        {
            ICommandExecutor? executor = _commandExecutors.FirstOrDefault(executor => executor.Handles(state.Command));
            if (executor is null)
            {
                _logger.LogError("No command executor found that handles command type {typeName}.", state.Command.GetType().FullName);

                return Prelude.Left<CommandError>(new ProgrammingError(
                    $"No command executor found that handles command type {state.Command.GetType().FullName}.",
                    2));
            }
        
            return (state, executor);
        };
    }

    
    private Abstractions.MyState<CommandState, CommandOutcome> RunExecutorWithState(ICommandExecutor commandExecutor)
    {
        return async state =>
        {
            // Update the state as needed
            state.ExecutedRetries = (state.ExecutedRetries ?? 0) + 1;

            // Log some details
            _logger.LogDebug("Executing command {CommandName} with {ExecutedRetries} retries.", state.Command.GetType().Name, state.ExecutedRetries);

            // Execute the command and return the outcome
            var outcome = await commandExecutor.ExecuteAsync(state.Command);
            
            Either<CommandError, (CommandState, CommandOutcome)> result = default;
            
            // Return the updated state and the outcome
            return result;
        };
    }
}