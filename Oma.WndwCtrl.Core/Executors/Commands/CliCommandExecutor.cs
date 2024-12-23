using LanguageExt;
using LanguageExt.Common;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Model.Commands;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public class CliCommandExecutor : ICommandExecutor<CliCommand>
{
    public Task<Either<CommandError, CommandOutcome>> ExecuteAsync(CliCommand command, CancellationToken cancelToken = default)
    {
        throw new NotImplementedException();
    }
}