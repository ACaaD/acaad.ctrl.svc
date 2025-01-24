using System.Diagnostics.CodeAnalysis;
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

namespace Oma.WndwCtrl.Core.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddCommandExecutors(
    this IServiceCollection services,
    IConfiguration configuration
  )
  {
    IConfigurationSection coreConfig = configuration.GetSection("Core");

    // TODO: Will cause problems when called multiple times.
    // Also: The name is wrong

    services.AddCliOutputParser()
      .AddSingleton<IParserLogger, CliParserLogger>()
      .Configure<CliParserLoggerOptions>(
        coreConfig.GetSection(CliParserLoggerOptions.SectionName)
      );

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
}