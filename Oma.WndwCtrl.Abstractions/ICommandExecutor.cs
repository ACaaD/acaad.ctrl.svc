using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

public interface ICommandExecutor
{
    bool Handles(ICommand command);
    
    Task<Either<FlowError, CommandOutcome>> ExecuteAsync(ICommand command, CancellationToken cancelToken = default);
}

public interface ICommandExecutor<in TCommand> : ICommandExecutor
{
    bool ICommandExecutor.Handles(ICommand command) => command is TCommand;
    
    async Task<Either<FlowError, CommandOutcome>> ICommandExecutor.ExecuteAsync(ICommand command, CancellationToken cancelToken)
    {
        if (command is not TCommand castedCommand)
        {
            return Left<FlowError>(new ProgrammingError($"Passed command is not of type {typeof(TCommand).Name}", Code: 1));
        }

        return await ExecuteAsync(castedCommand, cancelToken: cancelToken);
    }
    
    Task<Either<FlowError, CommandOutcome>> ExecuteAsync(TCommand command, CancellationToken cancelToken = default);
}