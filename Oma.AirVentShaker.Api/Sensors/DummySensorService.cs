using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Sensors;

public class DummySensorService(GlobalState GlobalState) : ISensorService
{
  public Task<CurrentGForces> ReadAsync(CancellationToken cancelToken) => Task.FromResult(
    new CurrentGForces(NetForce: 0)
    {
      TestStep = GlobalState.ActiveStep,
      TestDefinition = GlobalState.ActiveDefinition,
    }
  );
}