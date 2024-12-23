using LanguageExt;
using static LanguageExt.Prelude;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

public delegate Task<Either<CommandError, (S, A)>> MyState<S, A>(S state);

public static class OmaExtensions
{
    public static MyState<S, B> BindAsync<S, A, B>(this MyState<S, A> ma, Func<A, MyState<S, B>> f) =>
        state =>
        {
            try
            {
                return ma(state).BindAsync(pairA => f(pairA.Item2)(pairA.Item1));
            }
            catch(Exception e)
            {
                // TODO
                
                // return Left<CommandError>(new TechnicalError(e.Message, Code: 1337));
                throw new Exception();
            }
        };
    
    public async static Task<Either<CommandError, (S, A)>> RunAsync<S, A>(this MyState<S, A> ma, S state)
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

public class CommandState
{
    public ICommand Command { get; }

    public TimeSpan? ExecutionDuration { get; set; }
    public int? ExecutedRetries { get; set; }
    
    public CommandState(ICommand command)
    {
        Command = command;
    }
}

public interface ICommandExecutor
{
    bool Handles(ICommand command);
    
    Task<MyState<CommandState, CommandOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default);
}

public interface ICommandExecutor<in TCommand> : ICommandExecutor
{
    bool ICommandExecutor.Handles(ICommand command) => command is TCommand;
    
    async Task<MyState<CommandState, CommandOutcome>> ICommandExecutor.ExecuteAsync(ICommand command, CancellationToken cancelToken)
    {
        if (command is not TCommand castedCommand)
        {
            throw new Exception();

            // TODO
            // return Prelude.Left<CommandError>(new ProgrammingError($"Passed command is not of type {typeof(TCommand).Name}", Code: 1));
        }

        return await ExecuteAsync(castedCommand, cancelToken: cancelToken);
    }
    
    Task<MyState<CommandState, CommandOutcome>> ExecuteAsync(TCommand command, CancellationToken cancelToken = default);
}