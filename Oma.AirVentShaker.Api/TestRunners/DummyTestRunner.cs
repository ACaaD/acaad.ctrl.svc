using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;

namespace Oma.AirVentShaker.Api.TestRunners;

public class DummyTestRunner(GlobalState globalState, IAudioService audioService) : ITestRunner
{
  public async Task<TestSummary> ExecuteAsync(TestDefinition testDefinition, CancellationToken cancelToken)
  {
    Guid testId = Guid.NewGuid();
    globalState.ActiveDefinition = testDefinition;
    globalState.Stage = TestStage.Calibrate;

    foreach (TestStep testStep in testDefinition.Steps)
    {
      globalState.ActiveStep = testStep;

      await audioService.PlayAsync(
        new SineWaveDescriptor()
        {
          Frequency = testStep.Frequency,
          Amplitude = testStep.Amplitude,
        },
        testStep.Duration + TimeSpan.FromMilliseconds(milliseconds: 500),
        cancelToken
      );

      await Task.Delay(testStep.Duration, cancelToken);
    }

    globalState.Reset();

    return new TestSummary()
    {
      Id = testId,
      Status = "Finished",
    };
  }
}