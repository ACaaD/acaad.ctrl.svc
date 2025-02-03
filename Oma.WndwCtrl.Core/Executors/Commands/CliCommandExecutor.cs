using System.Collections.Concurrent;
using System.Diagnostics;
using JetBrains.Annotations;
using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Errors.Commands;
using Oma.WndwCtrl.Core.Model.Commands;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.Executors.Commands;

public partial class CliCommandExecutor(ILogger<CliCommandExecutor> logger) : ICommandExecutor<CliCommand>
{
  [MustDisposeResource]
  public async Task<Either<FlowError, CommandOutcome>> ExecuteAsync(
    CliCommand command,
    CancellationToken cancelToken = default
  )
  {
    try
    {
      Stopwatch swCliCommand = Stopwatch.StartNew();

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

      ConcurrentQueue<string> errorChunks = [];

      process.ErrorDataReceived += (_, e) =>
      {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
          errorChunks.Enqueue(e.Data);
        }
      };

      process.BeginErrorReadLine();
      string allText = await process.StandardOutput.ReadToEndAsync(cancelToken);

      await process.WaitForExitAsync(cancelToken);

      string outcome = process.ExitCode == 0 && errorChunks.IsEmpty
        ? allText
        : string.Join(Environment.NewLine, errorChunks);

      CommandOutcome result = new(outcome)
      {
        Success = process.ExitCode == 0 && errorChunks.IsEmpty,
      };

      LogExecution(logger, swCliCommand.Measure(), result.Success, result);

      return result;
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

  [LoggerMessage(
    Level = LogLevel.Trace,
    Message =
      "Finished executing cli command in {elapsed} (Success: {isSuccess}, Outcome={outcome})."
  )]
  public static partial void LogExecution(
    ILogger logger,
    TimeSpan elapsed,
    bool isSuccess,
    CommandOutcome outcome
  );
}