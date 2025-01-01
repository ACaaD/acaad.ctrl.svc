using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.MgmtApi.Model;

public class ServiceState
{
  public ServiceState(
    ILoggerFactory loggerFactory,
    IEnumerable<IApiService> apiServices,
    IEnumerable<IBackgroundService> backgroundServices
  )
  {
    BackgroundServices = backgroundServices
      .Select(svc =>
        new ServiceWrapper<IBackgroundService>(svc,
          loggerFactory.CreateLogger<ServiceWrapper<IBackgroundService>>())).ToList().AsReadOnly();

    ApiServices = apiServices
      .Select(svc =>
        new ServiceWrapper<IApiService>(svc, loggerFactory.CreateLogger<ServiceWrapper<IApiService>>()))
      .ToList().AsReadOnly();
  }

  public IReadOnlyList<IServiceWrapper<IBackgroundService>> BackgroundServices { get; init; }
  public IReadOnlyList<IServiceWrapper<IApiService>> ApiServices { get; init; }

  public IEnumerable<IServiceWrapper<IService>> All => BackgroundServices
    .Concat<IServiceWrapper<IService>>(ApiServices);
}