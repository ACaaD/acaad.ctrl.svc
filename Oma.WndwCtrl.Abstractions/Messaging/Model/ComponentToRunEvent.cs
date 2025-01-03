using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

[PublicAPI]
public record ComponentToRunEvent(IComponent Component) : Event
{
  public ComponentToRunEvent(IComponent component, Job? job) : this(component)
  {
    ScheduledFor = job?.ScheduledAt;
  }

  public override string Type => "Scheduling";
  public override string? ComponentName => Component.Name;
  public override string Name => nameof(ComponentToRunEvent);

  public DateTime? ScheduledFor { get; }
  public DateTime ProcessingStartedAt { get; } = DateTime.UtcNow;

  public TimeSpan? DelayedBy => ProcessingStartedAt - ScheduledFor;
}