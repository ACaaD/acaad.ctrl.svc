using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging;
using Oma.WndwCtrl.Messaging.Extensions;

namespace Oma.WndwCtrl.MgmtApi.Messaging;

public class EventLoggingService(ILogger<EventLoggingService> logger, Lazy<IMessageBus> messageBusLazy)
  : BackgroundServiceWrapper<EventLoggingService>, IMessageConsumer<IMessage>
{
  public bool IsSubscribedTo(IMessage message) => true;

  public Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default)
  {
    logger.LogInformation("Received message: {@Message}", message);
    return Task.CompletedTask;
  }

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(exception, "An unexpected error occurred.");
    return Task.CompletedTask;
  }

  protected override IServiceCollection ConfigureServices(IServiceCollection services) =>
    base.ConfigureServices(services)
      .AddMessageConsumer<EventLoggingService, IMessage>(registerConsumer: false)
      .AddSingleton(this);

  protected override IHost PostHostRun(IHost host, CancellationToken cancelToken = default)
  {
    IMessageBus messageBus = messageBusLazy.Value;
    ServiceProvider.StartConsumersAsync(messageBus, cancelToken);
    return base.PostHostRun(host, cancelToken);
  }
}