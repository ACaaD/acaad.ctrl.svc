using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.CommandProcessor;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.MgmtApi.Extensions;
using Oma.WndwCtrl.MgmtApi.Messaging;
using Oma.WndwCtrl.MgmtApi.Model;
using Oma.WndwCtrl.MgmtApi.Workers;
using Oma.WndwCtrl.Scheduling;

namespace Oma.WndwCtrl.MgmtApi;

public class MgmtApiService(MessageBusAccessor? messageBusAccessor)
  : WebApplicationWrapper<MgmtApiService>(messageBusAccessor)
{
  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    IServiceCollection result = base.ConfigureServices(services)
      .AddSingleton<ServiceState>()
      .AddComponentApi()
      .AddHostedService<ServiceWorker>()
      .AddBackgroundService<MessageBusService>()
      .AddBackgroundService<EventLoggingService>()
      .AddBackgroundService<SchedulingService>()
      .AddBackgroundService<CommandProcessingService>()
      // TODO: Should not always be added ? 
      .AddWindowsService();

    return result;
  }
}