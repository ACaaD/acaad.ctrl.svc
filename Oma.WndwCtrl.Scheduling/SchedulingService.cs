using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model.Scheduling;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;
using Oma.WndwCtrl.Scheduling.Services;

namespace Oma.WndwCtrl.Scheduling;

public class SchedulingService(
  ComponentConfigurationAccessor componentConfigurationAccessor,
  MessageBusAccessor messageBusAccessor
)
  : BackgroundServiceWrapper<SchedulingService>, IBackgroundService
{
  public static IEnumerable<ServiceDescriptor> Exposes =>
  [
    ServiceDescriptor.Singleton(new SchedulingConfigurationAccessor()),
  ];

  protected override IServiceCollection ConfigureServices(IServiceCollection services) =>
    services.AddMessageWriter().AddHostedService<SchedulingHostedService>();

  protected override IHost PostHostRun(IHost host, CancellationToken cancelToken = default)
  {
    base.PostHostRun(host, cancelToken);
    return host;
  }
}