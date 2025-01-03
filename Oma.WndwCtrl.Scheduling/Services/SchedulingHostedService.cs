using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;
using Oma.WndwCtrl.Scheduling.Model;

namespace Oma.WndwCtrl.Scheduling.Services;

public sealed class SchedulingHostedService(
  ILogger<SchedulingHostedService> logger,
  IJobList jobList,
  IOptions<SchedulingSettings> settingsOptions,
  IMessageBusWriter messageBusWriter
)
  : IHostedService, IAsyncDisposable
{
  private readonly CancellationTokenSource _cts = new();
  private PeriodicTimer? _timer;

  public async ValueTask DisposeAsync()
  {
    await CastAndDispose(_cts);

    if (_timer != null)
    {
      await CastAndDispose(_timer);
    }

    return;

    async static ValueTask CastAndDispose(IDisposable resource)
    {
      if (resource is IAsyncDisposable resourceAsyncDisposable)
      {
        await resourceAsyncDisposable.DisposeAsync();
      }
      else
      {
        resource.Dispose();
      }
    }
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    DateTime referenceDate = DateTime.UtcNow;

    logger.LogInformation("Starting scheduling service with reference date {refDate}.", referenceDate);
    int count = await jobList.LoadAsync(referenceDate, cancellationToken);
    logger.LogInformation("Found {count} jobs to schedule (Provider={type}).", count, jobList.Name);

    SchedulingSettings settings = settingsOptions.Value;
    _timer = new PeriodicTimer(settings.CheckInterval);
    logger.LogInformation("Creating periodic timer with check interval {interval}.", settings.CheckInterval);

    _ = ProcessJobsAsync();
    logger.LogInformation("Started job processing at {now}.", DateTime.UtcNow);
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping scheduling service.");
    _timer?.Dispose();
    await jobList.StoreAsync(cancellationToken);
  }

  private async Task ProcessJobsAsync()
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
    try
    {
      Stopwatch sw = Stopwatch.StartNew();
      logger.LogTrace("Checking for jobs to process.");

      DateTime referenceTime = DateTime.UtcNow;
      int processed = 0;

      await foreach (Job job in jobList.GetJobsToExecuteAsync(referenceTime, _cts.Token))
      {
        await ProcessJobAsync(job, _cts.Token);
        processed++;
      }

      logger.LogTrace("Processed {num} jobs in {elapsed}.", processed, sw.Measure());
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "An unexpected error occurred while processing jobs.");
    }
  }

  private async Task ProcessJobAsync(Job job, CancellationToken cancelToken = default)
  {
    ScheduledEvent message = new(job);
    logger.LogTrace("Identified job {job} for processing. Queuing event {event}.", job, message);

    await messageBusWriter.SendAsync(message, cancelToken);
  }
}