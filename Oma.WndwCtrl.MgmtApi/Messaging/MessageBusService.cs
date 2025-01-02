using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Extensions;

namespace Oma.WndwCtrl.MgmtApi.Messaging;

public class MessageBusService : BackgroundServiceWrapper<MessageBusService>, IBackgroundService
{
  private IMessageBus? _messageBus;

  private IMessageBus MessageBus =>
    _messageBus ?? throw new InvalidOperationException("MessageBus not initialized.");

  [SuppressMessage("ReSharper", "ArrangeStaticMemberQualifier")]
  public static IEnumerable<ServiceDescriptor> Exposes =>
  [
    // TODO: Make transient to force the factory method to be called all the time? 
    ServiceDescriptor.Singleton<Lazy<IMessageBus>>(
      _ =>
      {
        return new Lazy<IMessageBus>(
          () =>
          {
            IServiceProvider sp = BackgroundServiceWrapper<MessageBusService>.ServiceProvider;
            return sp.GetRequiredService<IMessageBus>();
          }
        );
      }
    ),
  ];

  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    services.AddMessageBus();
    return services;
  }

  protected override IHost PostHostRun(IHost host, CancellationToken cancelToken = default)
  {
    base.PostHostRun(host, cancelToken);
    _messageBus = ServiceProvider.StartMessageBus(cancelToken);
    return host;
  }
}