using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Configuration.Model;

namespace Oma.WndwCtrl.Configuration;

public class ConfigurationService(
  ComponentConfigurationAccessor componentConfigurationAccessor
)
  : IBackgroundService
{
  public async Task StartAsync(CancellationToken cancelToken = default, params string[] arg)
  {
    // TODO: Error handling
    componentConfigurationAccessor.Configuration =
      (await ComponentConfigurationAccessor.FromFileAsync(cancelToken)).Configuration;
  }

  public Task ForceStopAsync(CancellationToken cancelToken = default) => Task.CompletedTask;

  public Task WaitForShutdownAsync(CancellationToken cancelToken = default) => Task.CompletedTask;
}