using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Bus;

namespace Oma.WndwCtrl.Messaging.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddMessageBus(this IServiceCollection services)
    => services.AddSingleton<IMessageBus, MessageBus>().AddSingleton<MessageBusState>();

  public static IServiceCollection AddMessageConsumer<TMessage, TConsumer>(this IServiceCollection services)
    where TMessage : IMessage
    where TConsumer : class, IMessageConsumer
    => services;
}