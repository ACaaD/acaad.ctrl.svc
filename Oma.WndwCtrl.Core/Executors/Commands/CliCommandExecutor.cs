using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
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
  [SuppressMessage(
    "Reliability",
    "CA2000:Dispose objects before losing scope",
    Justification = "Must be disposed by caller."
  )]
  [MustDisposeResource]
  [SuppressMessage("ReSharper", "NotDisposedResource", Justification = "Method flagged as must-dispose.")]
  public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    CliCommand command,
    CancellationToken cancelToken = default
  )
  {
    try
    {
      ProcessStartInfo processStartInfo = new()
      {
        FileName = command.FileName,
        Arguments = command.Arguments,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
      };

      using Process? process = Process.Start(processStartInfo);

      if (process is null)
      {
        return Left<FlowError>(
          new CliCommandError("Could not obtain process instance.", isExceptional: true, isExpected: false)
        );
      }

      ConcurrentBag<string> errorChunks = [];

      process.ErrorDataReceived += (_, e) =>
      {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
          errorChunks.Add(e.Data);
        }
      };

      process.BeginErrorReadLine();
      string allText = await process.StandardOutput.ReadToEndAsync(cancelToken);

      await process.WaitForExitAsync(cancelToken);

      string outcome = process.ExitCode == 0 && errorChunks.IsEmpty
        ? allText
        : string.Join(Environment.NewLine, errorChunks);

      return Right(
        new CommandOutcome(outcome)
        {
          Success = process.ExitCode == 0 && errorChunks.IsEmpty,
        }
      );
    }
    catch (OperationCanceledException ex)
    {
      return Left<FlowError>(new OperationCancelledError(ex));
    }
    catch (Exception ex)
    {
      return Left<FlowError>(
        new TechnicalError("An unexpected technical error has occured executing CLI command.", Code: -1, ex)
      );
    }
  }
}