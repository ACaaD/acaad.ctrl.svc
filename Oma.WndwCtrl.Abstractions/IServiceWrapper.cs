using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.Abstractions;

public interface IServiceWrapper : IService
{
  string Name { get; }

  Guid ServiceGuid { get; }

  DateTime? StartedAt { get; }

  ServiceStatus Status { get; }

  Task StopAsync(CancellationToken cancelToken = default);
}

public interface IServiceWrapper<out TService> : IServiceWrapper
  where TService : class, IService
{
  TService Service { get; }

  string IServiceWrapper.Name => Service.GetType().Name;
}