using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Oma.WndwCtrl.Abstractions;

[PublicAPI]
public interface IService
{
  virtual static IEnumerable<ServiceDescriptor> Exposes { get; } = [];

  virtual static IServiceProvider ServiceProvider
  {
    get => throw new InvalidOperationException(
      "Access to the service provider is not allowed from the interface. It must be overriden."
    );
    set => throw new InvalidOperationException(
      "Access to the service provider is not allowed from the interface. It must be overriden."
    );
  }

  Task StartAsync(CancellationToken cancelToken = default, params string[] args);

  Task ForceStopAsync(CancellationToken cancelToken = default);

  Task WaitForShutdownAsync(CancellationToken cancelToken = default);
}