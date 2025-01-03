namespace Oma.WndwCtrl.Scheduling.Model;

public record SchedulingSettings
{
  public const string SettingsKey = "Scheduling";

  public TimeSpan CheckInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);
}