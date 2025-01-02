using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;

namespace Oma.WndwCtrl.MgmtApi.Messaging;

public class MessageBusService(MessageBusAccessor messageBusAccessor)
  : BackgroundServiceWrapper<MessageBusService>, IBackgroundService
{
  [SuppressMessage("ReSharper", "ArrangeStaticMemberQualifier")]
  public static IEnumerable<ServiceDescriptor> Exposes =>
  [
    ServiceDescriptor.Singleton(new MessageBusAccessor()),
    ServiceDescriptor.Transient<Lazy<IMessageBus?>>(
      _ =>
      {
        return new Lazy<IMessageBus?>(
          () =>
          {
            try
            {
              IServiceProvider sp = BackgroundServiceWrapper<MessageBusService>.ServiceProvider;
              return sp.GetRequiredService<IMessageBus>();
            }
            catch (ObjectDisposedException)
            {
              return null;
            }
            catch (Exception)
            {
              return null;
            }
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
    messageBusAccessor.MessageBus = ServiceProvider.StartMessageBus(cancelToken);
    return host;
  }
}