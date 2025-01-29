using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Metrics;
using Oma.WndwCtrl.CliOutputParser.Extensions;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.Core.Executors;
using Oma.WndwCtrl.Core.Executors.Commands;
using Oma.WndwCtrl.Core.Executors.Transformers;
using Oma.WndwCtrl.Core.FlowExecutors;
using Oma.WndwCtrl.Core.Interfaces;
using Oma.WndwCtrl.Core.Logger;
using Oma.WndwCtrl.Core.Metrics;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.Core.Model.Settings;

namespace Oma.WndwCtrl.Core.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddCommandExecutors(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    services.AddOptions();

    IConfigurationSection coreConfig = configuration.GetSection("Core");

    // TODO: Will cause problems when called multiple times.
    // Also: The name is wrong

    services.AddCliOutputParser()
      .AddSingleton<IParserLogger, CliParserLogger>()
      .Configure<CliParserLoggerOptions>(
        coreConfig.GetSection(CliParserLoggerOptions.SectionName)
      );

    ExtensionSettings extensions = [];
    coreConfig.GetSection(ExtensionSettings.SectionName).Bind(extensions);

    List<Assembly> extensionAssemblies = extensions.GetAssemblies();
    JsonExtensions.AddAssembliesToLoad(extensionAssemblies.ToList());
    AddExtensionExecutors(services, extensionAssemblies);

    services
      .AddSingleton<IAcaadCoreMetrics, AcaadCoreMetrics>()
      .AddSingleton<IExpressionCache, ExpressionCache>()
      .AddScoped<ICommandExecutor, CliCommandExecutor>()
      .AddScoped<IOutcomeTransformer, NoOpTransformer>()
      .AddScoped<IOutcomeTransformer, ParserTransformer>()
      .AddKeyedScoped<IFlowExecutor, AdHocFlowExecutor>(ServiceKeys.AdHocFlowExecutor)
      .AddKeyedScoped<ICommandExecutor, DelegatingCommandExecutor>(ServiceKeys.EntryCommandExecutor)
      .AddKeyedScoped<IRootTransformer, DelegatingTransformer>(ServiceKeys.RootTransformer);

    return services;
  }

  private static void AddExtensionExecutors(
    IServiceCollection services,
    List<Assembly> extensionAssemblies
  )
  {
    foreach (Assembly assembly in extensionAssemblies)
    {
      AddAllFromAssembly<ICommandExecutor>(services, assembly);
      AddAllFromAssembly<IOutcomeTransformer>(services, assembly);
    }
  }

  [PublicAPI]
  public static IServiceCollection AddAllFromAssembly<T>(
    IServiceCollection services,
    Assembly assembly,
    ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
  )
  {
    Type baseType = typeof(T);

    foreach (Type implType in assembly.GetTypes()
               .Where(t => t is { IsAbstract: false, } && t.IsAssignableTo(baseType)))
      services.Add(new ServiceDescriptor(baseType, implType, serviceLifetime));

    return services;
  }
}