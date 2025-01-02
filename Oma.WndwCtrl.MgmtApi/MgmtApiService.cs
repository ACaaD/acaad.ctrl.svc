using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.MgmtApi.Extensions;
using Oma.WndwCtrl.MgmtApi.Messaging;
using Oma.WndwCtrl.MgmtApi.Model;
using Oma.WndwCtrl.MgmtApi.Workers;

namespace Oma.WndwCtrl.MgmtApi;

public class MgmtApiService : WebApplicationWrapper<MgmtApiService>, IApiService
{
  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    IServiceCollection result = base.ConfigureServices(services)
      .AddSingleton<ServiceState>()
      .AddComponentApi()
      .AddHostedService<ServiceWorker>()
      .AddSingleton<MessageBusAccessor>()
      .AddBackgroundService<MessageBusService>()
      .AddBackgroundService<EventLoggingService>();

    return result;
  }
}