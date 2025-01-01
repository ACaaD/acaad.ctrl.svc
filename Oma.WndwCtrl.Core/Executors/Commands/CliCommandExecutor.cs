using System.Diagnostics;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Errors.Commands;
using Oma.WndwCtrl.Core.Model.Commands;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public class CliCommandExecutor : ICommandExecutor<CliCommand>
{
  public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    CliCommand command,
    CancellationToken cancelToken = default
  )
  {
    ProcessStartInfo processStartInfo = new()
    {
      FileName = command.FileName,
      Arguments = command.Arguments,
      CreateNoWindow = false,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
    };

    Process? process = Process.Start(processStartInfo);

    if (process is null)
    {
      return Left<FlowError>(new CliCommandError("Could not obtain process instance.", true, false));
    }

    await process.WaitForExitAsync(cancelToken);

    string allText = await process.StandardOutput.ReadToEndAsync(cancelToken);
    string errorText = await process.StandardError.ReadToEndAsync(cancelToken);

    return Right(new CommandOutcome()
    {
      Success = process.ExitCode == 0,
      OutcomeRaw = process.ExitCode == 0 ? allText
        : string.IsNullOrEmpty(errorText) ? allText : errorText,
    });
  }
}