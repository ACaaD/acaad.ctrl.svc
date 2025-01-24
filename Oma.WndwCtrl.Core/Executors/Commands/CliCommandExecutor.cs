using System.Diagnostics.CodeAnalysis;
using System.Text;
using CliWrap;
using JetBrains.Annotations;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
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
      StringBuilder stdOutBuffer = new();
      StringBuilder stdErrBuffer = new();

      Command cliBuilder = Cli.Wrap(command.FileName)
        .WithArguments(command.Arguments ?? string.Empty)
        .WithStandardOutputPipe(PipeTarget.ToStringBuilder(stdOutBuffer))
        .WithStandardErrorPipe(PipeTarget.ToStringBuilder(stdErrBuffer))
        .WithValidation(CommandResultValidation.None);

      if (command.WorkingDirectory is not null)
      {
        cliBuilder = cliBuilder.WithWorkingDirectory(command.WorkingDirectory);
      }

      CommandResult result = await cliBuilder.ExecuteAsync(CancellationToken.None, cancelToken);

      string stdOutRes = stdOutBuffer.ToString();
      string toUse;

      if (result.ExitCode == 0)
      {
        toUse = stdOutRes;
      }
      else
      {
        string stdErrRes = stdErrBuffer.ToString();

        toUse = string.IsNullOrEmpty(stdErrRes)
          ? stdOutRes
          : stdErrRes;
      }

      CommandOutcome outcome = new(toUse)
      {
        Success = result.ExitCode == 0,
      };

      return Right(
        outcome
      );
    }
    catch (OperationCanceledException ex)
    {
      return Left<FlowError>(new OperationCancelledError(ex));
    }
    catch (Exception ex)
    {
      return Left<FlowError>(new TechnicalError("An unexpected technical error has occured.", Code: -1, ex));
    }
  }
}