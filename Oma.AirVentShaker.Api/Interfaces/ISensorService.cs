using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Interfaces;

public record CurrentGForces
{
  public TestDefinition? TestDefinition { get; init; }

  public TestStep? TestStep { get; init; }

  public DateTime AsOf { get; } = DateTime.UtcNow;
}

public interface ISensorService
{
  Task<CurrentGForces> ReadAsync(CancellationToken cancelToken);
}