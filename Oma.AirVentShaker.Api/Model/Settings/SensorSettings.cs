namespace Oma.AirVentShaker.Api.Model.Settings;

public class AmplitudeCalculation
{
  public float DampeningFactor { get; init; } = 0.1f;

  public float MinDelta { get; init; } = -0.1f;
  public float MaxDelta { get; init; } = 0.1f;
}

public class AccelerometerConfiguration
{
  public int BusId { get; init; } = 0;

  public int Line { get; init; } = 1;
}

public class IoConfiguration
{
  public AccelerometerConfiguration Accelerometer { get; init; } = new();
}

public class SensorSettings
{
  public const string SectionName = "Sensor";

  public TimeSpan QueryInterval { get; init; } = TimeSpan.FromMilliseconds(milliseconds: 100);
  public int BatchSize { get; init; } = 2_500;

  public AmplitudeCalculation AmplitudeCalculation { get; init; } = new();

  public IoConfiguration Io { get; init; } = new();
}