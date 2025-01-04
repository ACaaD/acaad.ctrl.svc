using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[PublicAPI]
[Serializable]
public record ComponentExecutedEvent : ComponentExecutingEvent
{
  public ComponentExecutedEvent(
    ComponentExecutingEvent componentExecutingEvent,
    DateTime finishedAt,
    ComponentExecutionResult componentExecutionResult
  ) : base(
    componentExecutingEvent
  )
  {
    FinishedAt = finishedAt;
    ComponentExecutionResult = componentExecutionResult;
  }

  public DateTime FinishedAt { get; }
  public ComponentExecutionResult ComponentExecutionResult { get; }

  public override string Name => nameof(ComponentExecutedEvent);

  public TimeSpan Duration => FinishedAt - StartedAt;
}