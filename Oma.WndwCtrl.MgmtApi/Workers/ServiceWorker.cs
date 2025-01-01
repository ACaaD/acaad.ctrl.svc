using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.MgmtApi.Model;

namespace Oma.WndwCtrl.MgmtApi.Workers;

public class ServiceWorker : IHostedService
{
  private readonly ServiceState _serviceState;

  public ServiceWorker(ServiceState serviceState)
  {
    _serviceState = serviceState;
  }

  public async Task StartAsync(CancellationToken cancellationToken)
  {
    foreach (IServiceWrapper<IService>? service in _serviceState.All)
      await service.StartAsync(cancellationToken);
  }

  public async Task StopAsync(CancellationToken cancellationToken)
  {
    await Task.WhenAll(_serviceState.All.Select(svc => svc.ForceStopAsync(cancellationToken)));
  }
}