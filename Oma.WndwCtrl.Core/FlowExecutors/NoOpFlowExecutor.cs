using System.Text.Json;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Errors;
using Oma.WndwCtrl.Abstractions.Model;
using Oma.WndwCtrl.Core.Interfaces;
using static LanguageExt.Prelude;

namespace Oma.WndwCtrl.Core.FlowExecutors;

public class NoOpFlowExecutor : IFlowExecutor
{
  public Task<Either<FlowError, TransformationOutcome>> ExecuteAsync(
    ICommand command,
    CancellationToken cancelToken = default
  )
  {
    return Task.FromResult<Either<FlowError, TransformationOutcome>>(Right<TransformationOutcome>(
      new TransformationOutcome<ICommand>(command)
      {
        Success = true,
        OutcomeRaw = JsonSerializer.Serialize(command),
        Outcome = command,
      }));
  }
}