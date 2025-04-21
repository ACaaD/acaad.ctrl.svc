using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Interfaces;

public record CurrentGForces(float NetForce)
{
  private const double tolerance = 0.00000001;

  public TestDefinition? TestDefinition { get; init; }

  public TestStep? TestStep { get; init; }

  public DateTime AsOf { get; } = DateTime.UtcNow;

  public virtual bool Equals(CurrentGForces? other) => Math.Abs(NetForce - other?.NetForce ?? 0) < tolerance;

  public override int GetHashCode() => NetForce.GetHashCode();
}

public interface ISensorService
{
  Task<CurrentGForces> ReadAsync(CancellationToken cancelToken);
}