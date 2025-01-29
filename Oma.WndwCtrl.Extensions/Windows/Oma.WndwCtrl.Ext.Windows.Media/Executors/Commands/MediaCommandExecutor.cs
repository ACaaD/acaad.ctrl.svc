using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Ext.Windows.Media.Model.Commands;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Ext.Windows.Media.Executors.Commands;

public class MediaCommandExecutor : ICommandExecutor<MediaCommand>
{
  public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    MediaCommand command,
    CancellationToken cancelToken = default
  ) => Right(
    new CommandOutcome()
    {
      Success = true,
    }
  );
}