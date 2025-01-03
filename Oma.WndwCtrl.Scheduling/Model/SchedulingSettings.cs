namespace Oma.WndwCtrl.Scheduling.Model;

public record SchedulingSettings
{
  public static string SettingsKey = "Scheduling";

  public TimeSpan CheckInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);
}