using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model.Scheduling;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;
using Oma.WndwCtrl.Scheduling.Interfaces;
using Oma.WndwCtrl.Scheduling.Jobs;
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

  protected override IServiceCollection ConfigureServices(IServiceCollection services) => services
    .AddSingleton(componentConfigurationAccessor)
    .AddMessageWriter()
    .AddSingleton<IJobFactory, JobFactory>()
    .AddSingleton<IJobList, InMemoryJobList>()
    .AddHostedService<SchedulingHostedService>()
    .AddMessageConsumer<TriggerEventConsumer, IMessage>();

  protected override IHost PostHostRun(IHost host, CancellationToken cancelToken = default)
  {
    ServiceProvider.StartConsumersAsync(
      messageBusAccessor.MessageBus ?? throw new InvalidOperationException("MessageBus is not populated."),
      cancelToken
    );

    return base.PostHostRun(host, cancelToken);
  }
}