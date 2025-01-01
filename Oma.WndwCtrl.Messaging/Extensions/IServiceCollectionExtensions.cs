using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.Messaging.Consumers;
using Oma.WndwCtrl.Messaging.Model;

namespace Oma.WndwCtrl.Messaging.Extensions;

public sealed record WorkerMapping
{
  public Type ConsumerType { get; init; }

  public Type MessageType { get; init; }
}

public sealed record ConsumerMapping
{
  public object ServiceKey { get; init; }
}

public sealed record ChannelSettings
{
  public ushort Concurrency { get; init; } = 16;
}

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  private static Type ChannelType = typeof(Channel<IMessage>);

  public static IServiceCollection AddWorker<TConsumer, TMessage>(
    this IServiceCollection services,
    object serviceKey,
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
  )
    where TConsumer : IMessageConsumer<TMessage>
    where TMessage : IMessage
  {
    services.TryAddSingleton<ChannelWorkerFactory>();

    WorkerMapping mapping = new()
    {
      ConsumerType = typeof(TConsumer),
      MessageType = typeof(TMessage),
    };

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

    ServiceDescriptor consumerDescriptor = ServiceDescriptor.Describe(
      typeof(TConsumer),
      typeof(TConsumer),
      serviceLifetime
    );

    services.Add(consumerDescriptor);

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
    ServiceLifetime serviceLifetime = ServiceLifetime.Singleton
  )
    where TMessage : IMessage
    where TConsumer : class, IMessageConsumer<TMessage>
  {
    serviceKey ??= Guid.NewGuid();

    return services
      .AddLogging()
      .AddWorker<TConsumer, TMessage>(serviceKey, serviceLifetime)
      .AddSingleton(
        new ConsumerMapping()
        {
          ServiceKey = serviceKey,
        }
      );
  }
}