using System.Diagnostics;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;
using Oma.WndwCtrl.Scheduling.Model;

namespace Oma.WndwCtrl.Scheduling.Services;

public sealed class SchedulingHostedService(
  ILogger<SchedulingHostedService> logger,
  IJobList jobList,
  [FromKeyedServices(ServiceKeys.RootJobFactory)]
  IJobFactory jobFactory,
  IOptions<SchedulingSettings> settingsOptions,
  IMessageBusWriter messageBusWriter,
  ComponentConfigurationAccessor componentConfigurationAccessor
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

    IEnumerable<Job> jobsFromConfig = GenerateJobsFromConfiguration();

    logger.LogInformation("Starting scheduling service with reference date {refDate}.", referenceDate);
    int count = await jobList.MergeJobsAsync(jobsFromConfig, cancellationToken);
    logger.LogInformation("Found {count} jobs to schedule (Provider={type}).", count, jobList.Name);

    SchedulingSettings settings = settingsOptions.Value;

    logger.LogInformation("Creating periodic timer with check interval {interval}.", settings.CheckInterval);
    _timer = new PeriodicTimer(settings.CheckInterval);

    _ = ProcessJobsAsync();
    logger.LogInformation("Started job processing at {now}.", DateTime.UtcNow);
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    logger.LogInformation("Stopping scheduling service.");
    _timer?.Dispose();
    await jobList.StoreAsync(cancellationToken);
  }

  private IEnumerable<Job> GenerateJobsFromConfiguration()
  {
    DateTime referenceDate = DateTime.UtcNow;

    IEnumerable<Job> jobsFromConfig =
      componentConfigurationAccessor.Configuration.Triggers.OfType<ISchedulableTrigger>().Select(
        t => jobFactory.CreateJob(referenceDate, t)
      ).Somes();

    return jobsFromConfig;
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
    ScheduledEvent message = new(job); // TODO: Fill component name (somehow?)

    logger.LogDebug(
      "Job identified for processing. [Trigger={type}, ScheduleDelay={delay}, LastExecution={lastExecution}]",
      job.Trigger,
      DateTime.UtcNow - job.ScheduledAt,
      job.Previous.Map(prev => prev.ScheduledAt)
    );

    await messageBusWriter.SendAsync(message, cancelToken);
  }
}