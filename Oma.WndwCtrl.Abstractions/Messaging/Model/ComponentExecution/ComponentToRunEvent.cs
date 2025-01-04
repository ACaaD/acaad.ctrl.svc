using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[PublicAPI]
public sealed record ComponentToRunEvent(IComponent Component, ITrigger Trigger) : ComponentEvent(Component)
{
  public ComponentToRunEvent(IComponent component, ITrigger Trigger, Job? job) : this(
    component,
    Trigger
  )
  {
    ScheduledFor = job?.ScheduledAt;
  }

  public override string Type => "Scheduling";
  public override string ComponentName => Component.Name;
  public override string Name => nameof(ComponentToRunEvent);


  public DateTime? ScheduledFor { get; }
  public DateTime ProcessingStartedAt { get; } = DateTime.UtcNow;

  public TimeSpan? DelayedBy => ProcessingStartedAt - ScheduledFor;
}