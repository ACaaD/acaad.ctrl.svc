using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Oma.WndwCtrl.Messaging.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceProviderExtensions
{
  public static IMessageBus StartMessageBus(
    this IServiceProvider serviceProvider,
    CancellationToken cancelToken = default
  )
  {
    IMessageBus messageBus = serviceProvider.GetRequiredService<IMessageBus>();

    return messageBus;
  }

  public static IDisposable StartConsumers(
    this IServiceProvider serviceProvider,
    IMessageBus messageBus,
    CancellationToken cancelToken = default
  ) => new MemoryStream();
}