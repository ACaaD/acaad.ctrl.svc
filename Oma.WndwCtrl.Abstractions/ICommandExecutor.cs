using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

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
            return Either<CommandError, CommandOutcome>.Left(new ProgrammingError($"Passed command is not of type {typeof(TCommand).Name}", Code: 1));
        }

        return await ExecuteAsync(castedCommand, cancelToken: cancelToken);
    }
    
    Task<Either<CommandError, CommandOutcome>> ExecuteAsync(TCommand command, CancellationToken cancelToken = default);
}