namespace Oma.AirVentShaker.Api.Model.Settings;

public class SensorSettings
{
  public TimeSpan QueryInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);
  public int BatchSize { get; init; } = 25;
}