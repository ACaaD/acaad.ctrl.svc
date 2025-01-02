using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.MgmtApi.Model.Api;

[Serializable]
public record ServiceDescriptor
{
  public string Name { get; init; } = string.Empty;
  public Guid ServiceGuid { get; init; }
  public DateTime? StartedAt { get; init; }
  public ServiceStatus Status { get; init; }

  public static ServiceDescriptor FromServiceWrapper(IServiceWrapper serviceWrapper) => new()
  {
    Name = serviceWrapper.Name,
    StartedAt = serviceWrapper.StartedAt,
    Status = serviceWrapper.Status,
    ServiceGuid = serviceWrapper.ServiceGuid,
  };
}