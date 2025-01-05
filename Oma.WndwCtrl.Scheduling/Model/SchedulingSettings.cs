using JetBrains.Annotations;

namespace Oma.WndwCtrl.Scheduling.Model;

[PublicAPI]
public record SchedulingSettings
{
  public const string SettingsKey = nameof(SchedulingService);

  public TimeSpan CheckInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);

  public bool Active { get; init; } = true;
}