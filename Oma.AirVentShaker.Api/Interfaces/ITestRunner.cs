using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.Interfaces;

public interface ITestRunner
{
  Task<TestSummary> ExecuteAsync(TestDefinition testDefinition, CancellationToken cancelToken);
}