using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;

[PublicAPI]
public sealed record ComponentToRunEvent(IComponent Component, ITrigger Trigger)
  : ComponentEvent(Component), IHasSchedulingDelay
{
  public ComponentToRunEvent(IComponent component, ITrigger Trigger, Job? job) : this(
    component,
    Trigger
  )
  {
    ProcessingStartedAt = DateTime.UtcNow;
    ScheduledFor = job?.ScheduledAt;
  }

  public ComponentToRunEvent(ComponentToRunEvent other) : base(other)
  {
    Trigger = other.Trigger;
    ProcessingStartedAt = other.ProcessingStartedAt;
    ScheduledFor = other.ScheduledFor;
  }

  public override string Type => "Scheduling";
  public override string ComponentName => Component.Name;
  public override string Name => nameof(ComponentToRunEvent);


  public DateTime? ScheduledFor { get; }
  public DateTime ProcessingStartedAt { get; init; }

  public TimeSpan? DelayedBy => ProcessingStartedAt - ScheduledFor;
}