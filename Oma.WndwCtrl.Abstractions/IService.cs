namespace Oma.WndwCtrl.Abstractions;

public interface IService
{
  Task StartAsync(CancellationToken cancelToken = default, params string[] args);

  Task ForceStopAsync(CancellationToken cancelToken = default);

  Task WaitForShutdownAsync(CancellationToken cancelToken = default);
}