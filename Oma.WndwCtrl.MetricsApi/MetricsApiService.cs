﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.CoreAsp;
using Oma.WndwCtrl.Messaging.Bus;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Oma.WndwCtrl.MetricsApi;

public class MetricsApiService(MessageBusAccessor messageBusAccessor, IConfiguration rootConfiguration)
  : WebApplicationWrapper<MetricsApiService>(messageBusAccessor, rootConfiguration)
{
  protected override IServiceCollection ConfigureServices(IServiceCollection services)
  {
    OpenTelemetryBuilder otel = base.ConfigureServices(services).AddMetrics().AddOpenTelemetry();

    otel.ConfigureResource(res => res.AddService("ACAAD"));

    // Add Metrics for ASP.NET Core and our custom metrics and export to Prometheus
    otel.WithMetrics(
      metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddMeter("ACaaD.Core")
        .AddMeter("ACaaD.Processing")
        .AddMeter("ACaaD.Processing.Parser")
        .AddMeter("Microsoft.AspNetCore.Hosting")
        .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
        .AddMeter("Microsoft.Extensions.Diagnostics.ResourceMonitoring")
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
}