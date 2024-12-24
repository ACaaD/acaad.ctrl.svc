using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

public delegate Task<Either<CommandError, (S State, A Outcome)>> MyState<S, A>(S state);

public static class OmaExtensions
{
    public static MyState<S, B> BindAsync<S, A, B>(this MyState<S, A> ma, Func<A, MyState<S, B>> f) =>
        async state =>
        {
            try
            {
                return await ma(state).BindAsync(pairA => f(pairA.Item2)(pairA.Item1));
            }
            catch(Exception e)
            {
                return Left<CommandError>(new TechnicalError(e.Message, Code: 1337));
            }
        };
    
    public async static Task<Either<CommandError, (S State, A Outcome)>> RunAsync<S, A>(this MyState<S, A> ma, S state)
    {
        try
        {
            return await ma(state);
        }
        catch (Exception e)
        {
            return Left<CommandError>(new TechnicalError(e.Message, Code: 1337));
        }
    }
}

public record CommandState
{
    public ILogger Logger;
    public IEnumerable<ICommandExecutor> CommandExecutors;
    
    public ICommand Command { get; }
    
    public Option<DateTime> StartDate { get; init; }

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
    
    Task<Either<CommandError, CommandOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default);
}

public interface ICommandExecutor<in TCommand> : ICommandExecutor
{
    bool ICommandExecutor.Handles(ICommand command) => command is TCommand;
    
    async Task<Either<CommandError, CommandOutcome>> ICommandExecutor.ExecuteAsync(ICommand command, CancellationToken cancelToken)
    {
        if (command is not TCommand castedCommand)
        {
            return Left<CommandError>(new ProgrammingError($"Passed command is not of type {typeof(TCommand).Name}", Code: 1));
        }

        return await ExecuteAsync(castedCommand, cancelToken: cancelToken);
    }
    
    Task<Either<CommandError, CommandOutcome>> ExecuteAsync(TCommand command, CancellationToken cancelToken = default);
}