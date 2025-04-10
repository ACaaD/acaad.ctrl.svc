namespace Oma.AirVentShaker.Api.Model.Settings;

public class AmplitudeCalculation
{
  public float DampeningFactor { get; init; } = 0.8f;

  public float MinDelta { get; init; } = -0.1f;
  public float MaxDelta { get; init; } = 0.1f;
}

public class SensorSettings
{
  public const string SectionName = "Sensor";

  public TimeSpan QueryInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);
  public int BatchSize { get; init; } = 25;

  public AmplitudeCalculation AmplitudeCalculation { get; init; } = new();
}