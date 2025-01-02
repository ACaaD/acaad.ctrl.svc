using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.CoreAsp;

public class BackgroundServiceWrapper<TAssemblyDescriptor> : IBackgroundService
  where TAssemblyDescriptor : class, IBackgroundService
{
  private static IServiceProvider? _serviceProvider;

  protected static IServiceProvider ServiceProvider => _serviceProvider
                                                       ?? throw new InvalidOperationException(
                                                         "The WebApplicationWrapper has not been initialized properly."
                                                       );

  protected IHost? Host { get; private set; }

  public async Task StartAsync(CancellationToken cancelToken = default, params string[] args)
  {
#if DEBUG
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
#endif

    HostBuilder hostBuilder = new();
    hostBuilder.ConfigureServices((ctx, services) => ConfigureServices(services));

    Host = hostBuilder.Build();

    TAssemblyDescriptor.ServiceProvider = Host.Services;

    await Host.StartAsync(cancelToken);
    PostHostRun(Host, cancelToken);
  }

  public Task ForceStopAsync(CancellationToken cancelToken = default)
  {
    if (Host is not null)
    {
      return Host.StopAsync(cancelToken);
    }

    return Task.CompletedTask;
  }

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
    get => _serviceProvider;
    set => _serviceProvider = value;
  }

  [PublicAPI]
  protected virtual IServiceCollection ConfigureServices(IServiceCollection services) => services;

  [PublicAPI]
  protected virtual IHost PostHostRun(IHost host, CancellationToken cancelToken = default) =>
    host;
}