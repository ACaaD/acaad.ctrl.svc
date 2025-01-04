using Oma.WndwCtrl.Abstractions.Errors;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

public record ComponentCommandExecutionFailed : ComponentCommandExecutionFinished
{
  public ComponentCommandExecutionFailed(ComponentExecutingEvent componentExecuting, FlowError error) : base(
    componentExecuting
  )
  {
    Error = error;
  }

  public FlowError Error { get; }

  public override string Name => nameof(ComponentCommandExecutionFailed);
}