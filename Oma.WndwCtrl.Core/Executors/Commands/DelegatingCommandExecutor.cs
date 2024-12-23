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

class CommandState
{
    public ICommand Command { get; }

    public TimeSpan? ExecutionDuration { get; set; }
    public int? ExecutedRetries { get; set; }
    
    public CommandState(ICommand command)
    {
        Command = command;
    }
}

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
    
    public async Task<Either<CommandError, CommandOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default)
    {
        Stopwatch swExec = Stopwatch.StartNew();
        
        using IDisposable? ls = _logger.BeginScope(command);
        _logger.LogTrace("Received command to execute.");

        Either<CommandError, CommandOutcome> commandExecutor = (await FindCommandExecutor(command).AsTask()
                .BindAsync(executor => executor.ExecuteAsync(command, cancelToken)));
        
        Either<CommandError, CommandOutcome> outcome = await executor.ExecuteAsync(command, cancelToken).ConfigureAwait(false);
        
        _logger.LogDebug("Finished command in {elapsed} (Success={isSuccess})", swExec.Measure(), outcome.IsRight);

        return outcome;
    }

    private Either<CommandError, ICommandExecutor> FindCommandExecutor(ICommand command)
    {
        ICommandExecutor? executor = _commandExecutors.FirstOrDefault(executor => executor.Handles(command));
        if (executor is null)
        {
            _logger.LogError("No command executor found that handles command type {typeName}.", command.GetType().FullName);
            return Prelude.Left<CommandError>(new ProgrammingError($"No command executor found that handles command type {command.GetType().FullName}.", 2));     
        }
        
        return Prelude.Right(executor);
    }

    private CommandOutcome PopulateMetadata(CommandOutcome metadata)
    {
        throw new NotImplementedException();
    }
    
    private CommandError PopulateMetadata(CommandError metadata)
    {
        throw new NotImplementedException();
    }
}