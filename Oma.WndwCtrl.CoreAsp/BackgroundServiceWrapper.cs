using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace Oma.WndwCtrl.CoreAsp;

public class BackgroundServiceWrapper<TAssemblyDescriptor>(IConfiguration configuration) : IBackgroundService
  where TAssemblyDescriptor : class, IBackgroundService
{
  private static readonly string _serviceName = typeof(TAssemblyDescriptor).Name;

  [SuppressMessage(
    "ReSharper",
    "StaticMemberInGenericType",
    Justification = "Exactly the intended behaviour."
  )]
  private static IServiceProvider? _serviceProvider;

  private readonly string AcaadName =
    configuration.GetValue<string>("ACaaD:Name") ?? Guid.NewGuid().ToString();

  protected readonly string RunningInOs = configuration.GetValue<string>("ACaaD:OS") ?? "windows";
  private readonly bool UseOtlp = configuration.GetValue<bool>("ACaaD:UseOtlp");

  private IConfiguration? _configuration;

  protected static IServiceProvider ServiceProvider => _serviceProvider
                                                       ?? throw new InvalidOperationException(
                                                         "The WebApplicationWrapper has not been initialized properly."
                                                       );

  protected IConfiguration Configuration
  {
    get => _configuration ??
           throw new InvalidOperationException($"{nameof(Configuration)} is not populated.");
    private set => _configuration = value;
  }

  [PublicAPI]
  protected IHost? Host { get; private set; }

  private static string ServiceName => typeof(TAssemblyDescriptor).Name;

  public bool Enabled => !bool.TryParse(
    configuration.GetSection(ServiceName).GetValue<string>("Enabled") ?? "true",
    out bool enabled
  ) || enabled;

  public async Task StartAsync(CancellationToken cancelToken = default, params string[] args)
  {
#if DEBUG
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#endif

    Configuration = configuration;
    HostBuilder hostBuilder = new();

    hostBuilder.ConfigureHostConfiguration(builder => builder.AddConfiguration(configuration));

    hostBuilder.ConfigureLogging(
      (_, lb) =>
      {
        lb.ClearProviders();
        lb.SetMinimumLevel(LogLevel.Trace);

#if DEBUG
        lb.AddConsole();
#endif

        if (UseOtlp)
        {
          lb.AddOpenTelemetry(
            otelOptions =>
            {
              ResourceBuilder resourceBuilder =
                ResourceBuilder.CreateDefault().AddService(
                    _serviceName,
                    "ACaaD",
                    serviceInstanceId: AcaadName
                  )
                  .AddEnvironmentVariableDetector();

              otelOptions.SetResourceBuilder(resourceBuilder);

              otelOptions.IncludeScopes = true;
              otelOptions.IncludeFormattedMessage = true;
              otelOptions.ParseStateValues = true;

              otelOptions.AddOtlpExporter();
            }
          );
        }

        lb.AddConfiguration(configuration.GetSection("Logging"));
      }
    );

    hostBuilder.ConfigureServices((_, services) => ConfigureServices(services));

    Host = hostBuilder.Build();

    TAssemblyDescriptor.ServiceProvider = Host.Services;

    await PreHostRunAsync(cancelToken);
    await Host.StartAsync(cancelToken);
    PostHostRun(Host, cancelToken);
  }

  public Task ForceStopAsync(CancellationToken cancelToken = default) => Host is not null
    ? Host.StopAsync(cancelToken)
    : Task.CompletedTask;

  public async Task WaitForShutdownAsync(CancellationToken cancelToken = default)
  {
    if (Host is not null)
    {
      try
      {
        await Host.WaitForShutdownAsync(cancelToken);
      }
      finally
      {
        Host.Dispose();
        Host = null;
      }
    }
  }

  static IServiceProvider IService.ServiceProvider
  {
    get => ServiceProvider;
    set => _serviceProvider = value;
  }

  [PublicAPI]
  protected virtual IServiceCollection ConfigureServices(IServiceCollection services) => services;

  [PublicAPI]
  protected virtual IHost PostHostRun(IHost host, CancellationToken cancelToken = default) =>
    host;

  protected virtual Task PreHostRunAsync(CancellationToken cancelToken = default) => Task.CompletedTask;
}