using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

public delegate Either<CommandError, (S, A)> MyState<S, A>(S state);

public static class OmaExtensions
{
    public static MyState<S, B> BindAsync<S, A, B>(this MyState<S, A> ma, Func<A, MyState<S, B>> f) =>
        state =>
        {
            try
            {
                return ma(state).Bind(pairA => f(pairA.Item2)(pairA.Item1));
            }
            catch(Exception e)
            {
                // TODO
                
                // return Left<CommandError>(new TechnicalError(e.Message, Code: 1337));
                throw new Exception();
            }
        };
    
    public static Either<CommandError, (S, A)> RunAsync<S, A>(this MyState<S, A> ma, S state)
    {
        try
        {
            return ma(state);
        }
        catch (Exception e)
        {
            return Left<CommandError>(new TechnicalError(e.Message, Code: 1337));
        }
    }
}

public class CommandState
{
    public ILogger Logger;
    public IEnumerable<ICommandExecutor> CommandExecutors;
    
    public ICommand Command { get; }

    public TimeSpan? ExecutionDuration { get; set; }
    public int? ExecutedRetries { get; set; }
    
    public CommandState(ILogger logger, IEnumerable<ICommandExecutor> commandExecutors, ICommand command)
    {
        Logger = logger;
        CommandExecutors = commandExecutors;
        Command = command;
    }
}

public interface ICommandExecutor
{
    bool Handles(ICommand command);
    
    Either<CommandError, CommandOutcome> ExecuteAsync(ICommand command, CancellationToken cancelToken = default);
}

public interface ICommandExecutor<in TCommand> : ICommandExecutor
{
    bool ICommandExecutor.Handles(ICommand command) => command is TCommand;
    
    Either<CommandError, CommandOutcome> ICommandExecutor.ExecuteAsync(ICommand command, CancellationToken cancelToken)
    {
        if (command is not TCommand castedCommand)
        {
            return Left<CommandError>(new ProgrammingError($"Passed command is not of type {typeof(TCommand).Name}", Code: 1));
        }

        return ExecuteAsync(castedCommand, cancelToken: cancelToken);
    }
    
    Either<CommandError, CommandOutcome> ExecuteAsync(TCommand command, CancellationToken cancelToken = default);
}