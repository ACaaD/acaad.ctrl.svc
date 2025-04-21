using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;
using Oma.AirVentShaker.Api.Model.Events;
using Oma.AirVentShaker.Api.Model.Settings;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.AirVentShaker.Api.Messaging.Consumers;

public class AmplitudeAdjustingMessageConsumer : IMessageConsumer<GForceValueBatchEvent>
{
  private readonly IAudioService _audioService;
  private readonly GlobalState _globalState;
  private readonly ILogger<AmplitudeAdjustingMessageConsumer> _logger;

  private readonly long _maxAmplitudeDelta;
  private readonly long _minAmplitudeDelta;
  private readonly IOptions<SensorSettings> _sensorOptions;

  public AmplitudeAdjustingMessageConsumer(
    IOptions<SensorSettings> sensorOptions,
    GlobalState globalState,
    ILogger<AmplitudeAdjustingMessageConsumer> logger,
    IAudioService audioService
  )
  {
    _sensorOptions = sensorOptions;
    _globalState = globalState;
    _logger = logger;
    _audioService = audioService;
  }

  public bool IsSubscribedTo(IMessage message) =>
    _globalState.Stage == TestStage.Calibrate &&
    message is GForceValueBatchEvent;

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    _logger.LogError(
      exception,
      "An unexpected error occurred processing event {type}.",
      message.GetType().Name
    );

    return Task.CompletedTask;
  }

  public async Task OnMessageAsync(GForceValueBatchEvent message, CancellationToken cancelToken = default)
  {
    if (_globalState.Stage != TestStage.Calibrate || _globalState.ActiveStep is null)
    {
      return;
    }

    List<CurrentGForces> filteredMeasurements =
      message.DataPoints.Where(dp => dp.TestStep == _globalState.ActiveStep).ToList();

    float gForce = _globalState.ActiveStep.TargetGravitationalForce;

    await UpdateAmplitudeAsync(
      _globalState.ActiveStep,
      filteredMeasurements.Select(obj => obj.NetForce).ToList(),
      gForce,
      cancelToken
    );
  }

  private async Task UpdateAmplitudeAsync(
    TestStep step,
    List<float> measurements,
    float target,
    CancellationToken cancelToken = default
  )
  {
    AmplitudeCalculation calc = _sensorOptions.Value.AmplitudeCalculation;

    IEnumerable<float> deviations = measurements.Select(m => target - m);
    float devAverage = deviations.Average();

    float scaled = devAverage * calc.DampeningFactor;
    float clamped = Math.Clamp(scaled, calc.MinDelta, calc.MaxDelta);

    step.Amplitude += clamped;

    _logger.LogInformation(
      "Measured G-Forces: {avgG}. Updating amplitude to {Amplitude} (diff={Diff}) based on an average of {Avg} over {Cnt} values.",
      measurements.Average(),
      step.Amplitude,
      clamped,
      devAverage,
      measurements.Count
    );

    await _audioService.UpdateAmplitudeAsync(step.Amplitude, cancelToken);
  }

  private static IEnumerable<float> GetMeasurementsWithoutOutliers(IEnumerable<float> measurements)
  {
    List<float> sortedValues = measurements.OrderBy(v => v).ToList();

    int count = sortedValues.Count;
    float LQ = sortedValues[count / 4];
    float UQ = sortedValues[3 * count / 4];

    float IQR = UQ - LQ;

    float lowerBound = LQ - (long)(1.5 * IQR);
    float upperBound = UQ + (long)(1.5 * IQR);

    return sortedValues.Where(v => v >= lowerBound && v <= upperBound);
  }
}