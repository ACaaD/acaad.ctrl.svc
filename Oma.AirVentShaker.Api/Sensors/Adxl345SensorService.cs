using System.Device.Spi;
using System.Numerics;
using Iot.Device.Adxl345;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model;
using Oma.AirVentShaker.Api.Model.Settings;

namespace Oma.AirVentShaker.Api.Sensors;

public sealed class Adxl345SensorService : ISensorService, IDisposable
{
  private readonly SpiDevice _device;
  private readonly GlobalState _globalState;
  private readonly ILogger<Adxl345SensorService> _logger;
  private readonly Adxl345 _sensor;
  private readonly IOptions<SensorSettings> _sensorOptions;

  private Vector3? _baselineAcc = null;

  public Adxl345SensorService(
    ILogger<Adxl345SensorService> logger,
    IOptions<SensorSettings> sensorOptions,
    GlobalState globalState
  )
  {
    _logger = logger;
    _sensorOptions = sensorOptions;
    _globalState = globalState;

    IoConfiguration ioSettings = _sensorOptions.Value.Io;
    AccelerometerConfiguration accelerometerConfiguration = ioSettings.Accelerometer;

    _logger.LogInformation(
      "Opening accelerometer at bus {bus}, line {line}",
      accelerometerConfiguration.BusId,
      accelerometerConfiguration.Line
    );

    SpiConnectionSettings settings = new(accelerometerConfiguration.BusId, accelerometerConfiguration.Line)
    {
      ClockFrequency = Adxl345.SpiClockFrequency,
      Mode = Adxl345.SpiMode,
    };

    _device = SpiDevice.Create(settings);
    _sensor = new Adxl345(_device, GravityRange.Range02);

    _ = CalibrateSensorAsync();
  }

  public void Dispose()
  {
    _device.Dispose();
  }

  public async Task<CurrentGForces> ReadAsync(CancellationToken cancelToken)
  {
    if (_baselineAcc is null)
    {
      return new CurrentGForces(NetForce: 0)
      {
        TestDefinition = _globalState.ActiveDefinition,
        TestStep = _globalState.ActiveStep,
      };
    }

    Vector3 data = _sensor.Acceleration - _baselineAcc.Value;

    double magnitude = Math.Sqrt(data.X * data.X + data.Y * data.Y + data.Z * data.Z);

    CurrentGForces result = new((float)magnitude)
    {
      TestDefinition = _globalState.ActiveDefinition,
      TestStep = _globalState.ActiveStep,
    };

    return result;
  }

  private async Task CalibrateSensorAsync(CancellationToken cancelToken = default)
  {
    TimeSpan duration = TimeSpan.FromSeconds(seconds: 5);
    TimeSpan interval = TimeSpan.FromMilliseconds(milliseconds: 10);

    await Task.Delay(TimeSpan.FromSeconds(seconds: 1), cancelToken);

    List<Vector3> measurements = new();

    for (int i = 0; i < duration.TotalMilliseconds / interval.TotalMilliseconds; i++)
    {
      measurements.Add(_sensor.Acceleration);
      await Task.Delay(interval, cancelToken);
    }

    _baselineAcc = new Vector3(
      measurements.Average(v => v.X),
      measurements.Average(v => v.Y),
      measurements.Average(v => v.Z)
    );

    _logger.LogInformation(
      "Determined baseline vector for acceleration: [{X} {Y} {Z}] over {cnt} values",
      _baselineAcc.Value.X,
      _baselineAcc.Value.Y,
      _baselineAcc.Value.Z,
      measurements.Count
    );
  }
}