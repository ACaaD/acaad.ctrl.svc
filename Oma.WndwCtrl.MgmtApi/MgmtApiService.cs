using Oma.WndwCtrl.Api.Extensions;
using Oma.WndwCtrl.CommandProcessor;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.MetricsApi;
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
      .AddComponentApi(Configuration)
      .AddHostedService<ServiceWorker>()
      .AddApiService<MetricsApiService>()
      .AddBackgroundService<MessageBusService>()
      .AddBackgroundService<EventLoggingService>()
      .AddBackgroundService<SchedulingService>()
      .AddBackgroundService<CommandProcessingService>();

    string runningInOs = Configuration.GetValue<string>("ACaaD:OS") ?? "windows";

    switch (runningInOs)
    {
      case "windows":
        result.AddWindowsService();
        break;
      case "linux":
        result.AddSystemd();
        break;
      default:
        throw new InvalidOperationException($"OS value '{runningInOs}' is not supported.");
    }

    return result;
  }
}