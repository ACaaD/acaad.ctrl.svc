using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.TestRunners;

public class DummyTestRunner(IAudioService audioService) : ITestRunner
{
  public async Task<TestSummary> ExecuteAsync(TestDefinition testDefinition, CancellationToken cancelToken)
  {
    Guid testId = Guid.NewGuid();

    foreach (TestStep testStep in testDefinition.Steps)
    {
      await audioService.PlayAsync(
        new SineWaveDescriptor()
        {
          Frequency = testStep.Frequency,
          Amplitude = 0.5f,
        },
        testStep.Duration,
        cancelToken
      );

      await Task.Delay(testStep.Duration, cancelToken);
    }

    return new TestSummary()
    {
      Id = testId,
      Status = "Finished",
    };
  }
}