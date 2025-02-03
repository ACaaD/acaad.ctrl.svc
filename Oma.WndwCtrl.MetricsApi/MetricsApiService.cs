using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Oma.WndwCtrl.Core.Model.Settings;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using Oma.WndwCtrl.MetricsApi.Metrics;
using OpenTelemetry;
using OpenTelemetry.Metrics;

namespace Oma.WndwCtrl.MetricsApi;

public class MetricsApiService(MessageBusAccessor messageBusAccessor, IConfiguration rootConfiguration)
  : WebApplicationWrapper<MetricsApiService>(messageBusAccessor, rootConfiguration)
{
  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    services.AddSingleton<MetadataMetrics>()
      .Configure<GeneralSettings>(Configuration.GetSection(GeneralSettings.SectionName));

    OpenTelemetryBuilder otel = base.ConfigureServices(services).AddMetrics().AddOpenTelemetry();

    // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
    otel.WithMetrics(
      metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("ACaaD.Metadata")
        .AddMeter("ACaaD.Core")
        .AddMeter("ACaaD.Processing")
        .AddMeter("ACaaD.Processing.Parser")
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Routing")
        .AddMeter("Microsoft.AspNetCore.Http.Connections")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        .AddMeter("Microsoft.AspNetCore.Diagnostics")
        .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
        .AddMeter("System.Net.Http")
        .AddMeter("System.Net.NameResolution")
        .AddMeter("System.Runtime")
        .AddPrometheusExporter()
    );

    return services;
  }

  protected override WebApplication PreAppRun(WebApplication app)
  {
    app.MapPrometheusScrapingEndpoint();
    return base.PreAppRun(app);
  }

  protected override WebApplication PostAppRun(WebApplication app, CancellationToken cancelToken = default)
  {
    MetadataMetrics metrics = ServiceProvider.GetRequiredService<MetadataMetrics>();
    IOptions<GeneralSettings> settingOpts = ServiceProvider.GetRequiredService<IOptions<GeneralSettings>>();

    metrics.PopulateMetadata(settingOpts.Value);

    return base.PostAppRun(app, cancelToken);
  }
}