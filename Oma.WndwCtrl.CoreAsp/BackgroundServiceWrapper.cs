using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.CoreAsp;

public class BackgroundServiceWrapper<TAssemblyDescriptor>(IConfiguration configuration) : IBackgroundService
  where TAssemblyDescriptor : class, IBackgroundService
{
  [SuppressMessage(
    "ReSharper",
    "StaticMemberInGenericType",
    Justification = "Exactly the intended behaviour."
  )]
  private static IServiceProvider? _serviceProvider;

  protected readonly string RunningInOs = configuration.GetValue<string>("ACaaD:OS") ?? "windows";

  private IConfiguration _configuration;

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
      (_, logging) =>
      {
        logging.SetMinimumLevel(LogLevel.Trace);
        logging.AddConsole();

        logging.AddConfiguration(configuration.GetSection("Logging"));
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