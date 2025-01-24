using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.CliOutputParser.Interfaces;
using Oma.WndwCtrl.CliOutputParser.Logger;
using Oma.WndwCtrl.CliOutputParser.Metrics;

namespace Oma.WndwCtrl.CliOutputParser.Extensions;

[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Extension methods")]
public static class IServiceCollectionExtensions
{
  public static IServiceCollection AddCliOutputParser(
    this IServiceCollection services
  )
  {
    services.AddMetrics();

    services.AddSingleton<ICliOutputParser, CliOutputParserImpl>()
      .AddSingleton<IParserLogger, NoOpLogger>()
      .AddSingleton<TransformationTreeCache>()
      .AddSingleton<ParserMetrics>();

    return services;
  }
}