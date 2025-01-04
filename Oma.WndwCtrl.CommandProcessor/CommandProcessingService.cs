using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oma.WndwCtrl.Abstractions.Messaging.Model.ComponentExecution;
using Oma.WndwCtrl.CommandProcessor.Messaging;
using Oma.WndwCtrl.Core.Extensions;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Extensions;

namespace Oma.WndwCtrl.CommandProcessor;

public class CommandProcessingService(IConfiguration configuration, MessageBusAccessor messageBusAccessor)
  : BackgroundServiceWrapper<CommandProcessingService>(configuration)
{
  protected override IServiceCollection ConfigureServices(IServiceCollection services) => services
    .AddCommandExecutors()
    .UseMessageBus(messageBusAccessor)
    .AddMessageConsumer<ComponentToExecuteMessageConsumer, ComponentToRunEvent>()
    .AddMessageWriter();

  protected override IHost PostHostRun(IHost host, CancellationToken cancelToken = default)
  {
    host.Services.StartConsumersAsync(
      messageBusAccessor.MessageBus ?? throw new InvalidOperationException("MessageBus is not populated."),
      cancelToken
    );

    return base.PostHostRun(host, cancelToken);
  }
}