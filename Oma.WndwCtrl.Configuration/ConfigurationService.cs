using Microsoft.Extensions.Configuration;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.CoreAsp;

namespace Oma.WndwCtrl.Configuration;

public class ConfigurationService(
  IConfiguration configuration,
  ComponentConfigurationAccessor componentConfigurationAccessor
)
  : BackgroundServiceWrapper<ConfigurationService>(configuration), IBackgroundService
{
  protected async override Task PreHostRunAsync(CancellationToken cancelToken = default)
  {
    await base.PreHostRunAsync(cancelToken);

    // TODO: Error handling
    componentConfigurationAccessor.Configuration =
      (await ComponentConfigurationAccessor.FromFileAsync(RunningInOs, cancelToken)).Configuration;
  }
}