using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

public record Job(ISchedulableTrigger Trigger, DateTime ScheduledAt)
{
  public Option<Job> Previous { get; init; } = Option<Job>.None;
}