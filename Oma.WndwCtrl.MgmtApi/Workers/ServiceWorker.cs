using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.MgmtApi.Model;

namespace Oma.WndwCtrl.MgmtApi.Workers;

public class ServiceWorker(ServiceState serviceState) : IHostedService
{
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    foreach (IServiceWrapper<IService>? service in serviceState.All.Where(s => s.Enabled))
      await service.StartAsync(cancellationToken);
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await Task.WhenAll(serviceState.All.Select(svc => svc.ForceStopAsync(cancellationToken)));
  }
}