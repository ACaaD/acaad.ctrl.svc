using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Consumers;
using Oma.WndwCtrl.Messaging.Model;

namespace Oma.WndwCtrl.Messaging.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  private static readonly Type ChannelType = typeof(Channel<IMessage>);

  internal static IServiceCollection AddWorker<TConsumer, TMessage>(
    this IServiceCollection services,
    object serviceKey,
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton,
    bool registerConsumer = true
  )
    where TConsumer : IMessageConsumer<TMessage>
    where TMessage : IMessage
  {
    services.TryAddSingleton<ChannelWorkerFactory>();

    WorkerMapping mapping = new(typeof(TConsumer), typeof(TMessage));

    services.AddKeyedSingleton(serviceKey, mapping)
      .AddKeyedSingleton(serviceKey, new ChannelSettings());

    ServiceDescriptor channelDescriptor = ServiceDescriptor.DescribeKeyed(
      ChannelType,
      serviceKey,
      (_1, _2) => Channel.CreateUnbounded<IMessage>(),
      serviceLifetime
    );

    services.TryAdd(channelDescriptor);

    Type channelWorkerType = typeof(ChannelWorker<TConsumer, TMessage>);

    ServiceDescriptor workerDescriptor = ServiceDescriptor.DescribeKeyed(
      channelWorkerType,
      serviceKey,
      channelWorkerType,
      serviceLifetime
    );

    services.Add(workerDescriptor);

    ServiceDescriptor interfaceDescriptor = ServiceDescriptor.DescribeKeyed(
      typeof(IChannelWorker),
      serviceKey,
      (sp, key) => sp.GetRequiredService<ChannelWorkerFactory>().CreateChannelWorker(key ?? serviceKey),
      serviceLifetime
    );

    services.Add(interfaceDescriptor);

    if (registerConsumer)
    {
      ServiceDescriptor consumerDescriptor = ServiceDescriptor.Describe(
        typeof(TConsumer),
        typeof(TConsumer),
        serviceLifetime
      );

      services.Add(consumerDescriptor);
    }

    return services;
  }

  public static IServiceCollection AddMessageBus(this IServiceCollection services)
    => services
      .AddLogging()
      .AddSingleton<IMessageBus, MessageBus>()
      .AddSingleton<MessageBusState>()
      .AddWorker<FanOutMessageConsumer, IMessage>(ServiceKeys.MessageBus);

  public static IServiceCollection AddMessageConsumer<TConsumer, TMessage>(
    this IServiceCollection services,
    object? serviceKey = null,
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton,
    bool registerConsumer = true
  )
    where TMessage : IMessage
    where TConsumer : class, IMessageConsumer<TMessage>
  {
    serviceKey ??= Guid.NewGuid();

    return services
      .AddLogging()
      .AddWorker<TConsumer, TMessage>(serviceKey, serviceLifetime, registerConsumer)
      .AddSingleton(new ConsumerMapping(serviceKey));
  }
}