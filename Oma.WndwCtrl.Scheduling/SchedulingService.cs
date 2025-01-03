using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.Core.Model.Scheduling;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;
using Oma.WndwCtrl.Scheduling.Interfaces;
using Oma.WndwCtrl.Scheduling.JobFactories;
using Oma.WndwCtrl.Scheduling.Jobs;
using Oma.WndwCtrl.Scheduling.Model;
using Oma.WndwCtrl.Scheduling.Services;

namespace Oma.WndwCtrl.Scheduling;

public class SchedulingService(
  IConfiguration configuration,
  ComponentConfigurationAccessor componentConfigurationAccessor,
  MessageBusAccessor messageBusAccessor
)
  : BackgroundServiceWrapper<SchedulingService>(configuration), IBackgroundService
{
  private readonly IConfiguration _configuration = configuration;

  public static IEnumerable<ServiceDescriptor> Exposes =>
  [
    ServiceDescriptor.Singleton(new SchedulingConfigurationAccessor()),
  ];

  protected override IServiceCollection ConfigureServices(IServiceCollection services) => services
    .AddSingleton(componentConfigurationAccessor)
    .UseMessageBus(messageBusAccessor)
    .AddMessageWriter()
    .AddKeyedSingleton<IJobFactory, DelegatingJobFactory>(ServiceKeys.RootJobFactory)
    .AddSingleton<IJobFactory, RateJobFactory>()
    .AddSingleton<IJobList, InMemoryJobList>()
    .AddHostedService<SchedulingHostedService>()
    .Configure<SchedulingSettings>(_configuration.GetSection(SchedulingSettings.SettingsKey))
    .AddMessageConsumer<TriggerEventConsumer, IMessage>();

  protected override IHost PostHostRun(IHost host, CancellationToken cancelToken = default)
  {
    host.Services.StartConsumersAsync(
      messageBusAccessor.MessageBus ?? throw new InvalidOperationException("MessageBus is not populated."),
      cancelToken
    );

    return base.PostHostRun(host, cancelToken);
  }
}