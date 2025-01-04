using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[Serializable]
[PublicAPI]
public abstract record ComponentCommandExecutionFinished : ComponentExecutingEvent
{
  protected ComponentCommandExecutionFinished(ComponentExecutingEvent componentExecuting) : base(
    componentExecuting
  )
  {
    FinishedAt = DateTime.UtcNow;
  }

  public DateTime FinishedAt { get; }

  public TimeSpan Duration => FinishedAt - StartedAt;

  public override string Name => nameof(ComponentCommandExecutionFinished);
}