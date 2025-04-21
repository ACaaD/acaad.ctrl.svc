using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.AirVentShaker.Api.Interfaces;
using Oma.AirVentShaker.Api.Model.Events;
using Oma.AirVentShaker.Api.Model.Settings;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.AirVentShaker.Api.Workers;

public class HighResSensorWorker(
  ILogger<SensorWorker> logger,
  IOptions<SensorSettings> sensorOptions,
  ISensorService sensorService,
  IMessageBusWriter messageBusWriter
) : IHostedService
{
  private readonly CancellationTokenSource _cts = new();

  private ConcurrentBag<CurrentGForces> _active = new();
  private bool _isRunning;
  private CurrentGForces? _last;
  private ConcurrentBag<CurrentGForces> _other = new();

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    _isRunning = true;

    _ = ProcessAsync();
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    _isRunning = false;

    await _cts.CancelAsync();
    _cts.Dispose();
  }

  private async Task ProcessAsync()
  {
    int batchSize = sensorOptions.Value.BatchSize;

    await Task.Delay(TimeSpan.FromSeconds(seconds: 7));
    logger.LogInformation("Starting to read sensor values.");

    while (_isRunning)
      try
      {
        CurrentGForces currentReading = await sensorService.ReadAsync(_cts.Token);

        _active.Add(currentReading);

        if (_active.Count >= batchSize)
        {
          await QueueBatchAsync();
        }

        await Task.Delay(TimeSpan.FromMicroseconds(microseconds: 100));
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An unexpected error occurred.");
      }
  }

  private async Task QueueBatchAsync()
  {
    ConcurrentBag<CurrentGForces> fullBatch = Interlocked.Exchange(ref _active, _other);

    GForceValueBatchEvent @event = new()
    {
      DataPoints = fullBatch.ToList(),
    };

    fullBatch.Clear();

    await messageBusWriter.SendAsync(@event, _cts.Token);
  }
}