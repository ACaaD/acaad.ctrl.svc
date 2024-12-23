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

    private readonly MyState<CommandState, CommandOutcome> _callChain;
    
    public bool Handles(ICommand command) => true;
    
    public DelegatingCommandExecutor(
        ILogger<DelegatingCommandExecutor> logger,
        IEnumerable<ICommandExecutor> commandExecutors
    )
    {
        _logger = logger;
        _commandExecutors = commandExecutors;
        
        _callChain = FindCommandExecutor()
            .BindAsync(RunExecutor);
    }
    
    // public delegate Either<CommandError, (S, A)> State<S, A>(S state);
    
    public Either<CommandError, CommandOutcome> ExecuteAsync(ICommand command, CancellationToken cancelToken = default)
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(command);
        _logger.LogTrace("Received command to execute.");

        CommandState initialState = new(_logger, _commandExecutors, command);
        
        var outcomeWithState = _callChain.RunAsync(initialState);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcomeWithState);
        
        return outcomeWithState.BiMap(
            tuple => tuple.Item2,
            err => err
        );
    }
    
    private static MyState<CommandState, ICommandExecutor> FindCommandExecutor()
    {
        return state =>
        {
            ICommandExecutor? executor = state.CommandExecutors.FirstOrDefault(executor => executor.Handles(state.Command));
            
            if (executor is null)
            {
                state.Logger.LogError("No command executor found that handles command type {typeName}.", state.Command.GetType().FullName);

                return Prelude.Left<CommandError>(new ProgrammingError(
                    $"No command executor found that handles command type {state.Command.GetType().FullName}.",
                    2));
            }
        
            return (state, executor);
        };
    }
    
    private static Abstractions.MyState<CommandState, CommandOutcome> RunExecutor(ICommandExecutor commandExecutor)
    {
        return state =>
        {
            state.Logger.LogDebug("Executing command {CommandName} with {ExecutedRetries} retries.", state.Command.GetType().Name, state.ExecutedRetries);
            
            var either = commandExecutor.ExecuteAsync(state.Command);
            
            return either.BiMap(
                oc => (state, oc),
                err => err
            );
        };
    }
}