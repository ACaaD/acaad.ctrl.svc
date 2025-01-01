using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Configuration.Model;

namespace Oma.WndwCtrl.Configuration.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddConfiguration(this IServiceCollection services)
  {
    services.TryAddSingleton<IBackgroundService, ConfigurationService>();
    services.TryAddSingleton<ComponentConfigurationAccessor>();
    return services;
  }
}