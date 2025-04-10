namespace Oma.AirVentShaker.Api.Model;

public class TestStep
{
  public float Frequency { get; init; }

  public TimeSpan Duration { get; init; }

  public decimal TargetGravitationalForce { get; init; }
}