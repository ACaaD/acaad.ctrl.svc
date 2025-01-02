using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.MgmtApi.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddApiService<TService>(this IServiceCollection services)
    where TService : class, IApiService
  {
    services.AddSingleton<IApiService, TService>();

    foreach (ServiceDescriptor serviceDescriptor in TService.Exposes) services.Add(serviceDescriptor);

    return services;
  }

  public static IServiceCollection AddBackgroundService<TService>(this IServiceCollection services)
    where TService : class, IBackgroundService
  {
    services.AddSingleton<IBackgroundService, TService>();

    foreach (ServiceDescriptor serviceDescriptor in TService.Exposes) services.Add(serviceDescriptor);

    return services;
  }
}