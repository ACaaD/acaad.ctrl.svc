using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model.Events;
using Oma.AirVentShaker.Api.Model.Settings;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.AirVentShaker.Api.Workers;

public class SensorWorker(
  ILogger<SensorWorker> logger,
  IOptions<SensorSettings> sensorOptions,
  ISensorService sensorService,
  IMessageBusWriter messageBusWriter
) : IHostedService
{
  private readonly CancellationTokenSource _cts = new();

  private ConcurrentBag<CurrentGForces> _active = new();
  private ConcurrentBag<CurrentGForces> _other = new();
  private PeriodicTimer? _timer;

  public Task StartAsync(CancellationToken cancellationToken)
  {
    SensorSettings settings = sensorOptions.Value;

    _timer = new PeriodicTimer(settings.QueryInterval);
    _ = ReadSensorAsync();

    return Task.CompletedTask;
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await _cts.CancelAsync();
    _cts.Dispose();
  }

  private async Task ReadSensorAsync()
  {
    try
    {
      while (await (_timer?.WaitForNextTickAsync(_cts.Token) ?? ValueTask.FromResult(result: false)))
        await ProcessTimerTickAsync();
    }
    catch (OperationCanceledException)
    {
      logger.LogInformation("Scheduling service canceled.");
    }
  }

  private async Task ProcessTimerTickAsync()
  {
    int batchSize = sensorOptions.Value.BatchSize;

    try
    {
      CurrentGForces currentReading = await sensorService.ReadAsync(_cts.Token);
      _active.Add(currentReading);

      if (_active.Count >= batchSize)
      {
        ConcurrentBag<CurrentGForces> fullBatch = Interlocked.Exchange(ref _active, _other);

        GForceValueBatchEvent @event = new()
        {
          DataPoints = fullBatch.ToList(),
        };

        fullBatch.Clear();
        logger.LogDebug("Switched buffers.");

        await messageBusWriter.SendAsync(@event, _cts.Token);
      }
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An unexpected error occurred while processing jobs.");
    }
  }
}