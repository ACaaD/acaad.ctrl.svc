namespace Oma.WndwCtrl.Scheduling.Model;

public record SchedulingSettings
{
  public TimeSpan CheckInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);
}