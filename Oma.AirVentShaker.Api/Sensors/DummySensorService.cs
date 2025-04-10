using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Sensors;

public class DummySensorService(GlobalState globalState) : ISensorService
{
  public async Task<CurrentGForces> ReadAsync(CancellationToken cancelToken) => new()
  {
    TestDefinition = globalState.ActiveDefinition,
    TestStep = globalState.ActiveStep,
  };
}