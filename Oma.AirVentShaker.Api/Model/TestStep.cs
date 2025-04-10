namespace Oma.AirVentShaker.Api.Model;

public class TestStep
{
  public float Frequency { get; init; }

  public TimeSpan Duration { get; init; }

  public float TargetGravitationalForce { get; init; }

  public float Amplitude { get; set; } = 0.05f;

  public override string ToString() =>
    $"Freq={Frequency}Hz;Dur={Duration};Target={TargetGravitationalForce}G;CurrA={Amplitude}";
}