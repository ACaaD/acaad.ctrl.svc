using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Model.Commands;

using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public class DummyCommandExecutor : ICommandExecutor<DummyCommand>
{
    public Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
        DummyCommand command, 
        CancellationToken cancelToken = default
    )
    {
        Either<FlowError, CommandOutcome> result;

        string message = string.Join(Environment.NewLine, command.Returns);
        
        if (command.SimulateFailure)
        {
            result = Left(new FlowError(message, isExceptional: command.IsExceptional, isExpected: command.IsExpected));
        }
        else
        {
            result = Right<CommandOutcome>(new()
            {
                OutcomeRaw = message
            });            
        }

        return Task.FromResult(result);
    }
}