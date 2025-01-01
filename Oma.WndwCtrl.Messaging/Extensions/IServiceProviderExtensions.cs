using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Model;

namespace Oma.WndwCtrl.Messaging.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceProviderExtensions
{
  public static IMessageBus StartMessageBus(
    this IServiceProvider serviceProvider,
    CancellationToken cancelToken = default
  )
  {
    IChannelWorker worker =
      serviceProvider.GetRequiredKeyedService<IChannelWorker>(ServiceKeys.MessageBus);

    IMessageBus messageBus = serviceProvider.GetRequiredService<IMessageBus>();

    _ = worker.ProcessUntilCompletedAsync(cancelToken);

    return messageBus;
  }

  public static Task StartConsumersAsync(
    this IServiceProvider serviceProvider,
    IMessageBus messageBus,
    CancellationToken cancelToken = default
  )
  {
    IEnumerable<ConsumerMapping> consumerMappings = serviceProvider.GetServices<ConsumerMapping>();

    List<Task> consumerTasks = [];

    foreach (ConsumerMapping mapping in consumerMappings)
    {
      Channel<IMessage> channel =
        serviceProvider.GetRequiredKeyedService<Channel<IMessage>>(mapping.ServiceKey);

      IChannelWorker worker = serviceProvider.GetRequiredKeyedService<IChannelWorker>(mapping.ServiceKey);

      messageBus.Register(mapping.ServiceKey.ToString()!, channel);
      consumerTasks.Add(worker.ProcessUntilCompletedAsync(cancelToken));
    }

    return Task.WhenAll(consumerTasks);
  }
}