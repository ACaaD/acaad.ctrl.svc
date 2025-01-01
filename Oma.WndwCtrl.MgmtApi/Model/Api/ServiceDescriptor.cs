using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Model;

namespace Oma.WndwCtrl.MgmtApi.Model.Api;

public record ServiceDescriptor
{
  public string Name { get; init; } = string.Empty;
  public Guid ServiceGuid { get; init; }
  public DateTime? StartedAt { get; init; }
  public ServiceStatus Status { get; init; }

  public static ServiceDescriptor FromServiceWrapper(IServiceWrapper serviceWrapper)
  {
    return new ServiceDescriptor
    {
      Name = serviceWrapper.Name,
      StartedAt = serviceWrapper.StartedAt,
      Status = serviceWrapper.Status,
      ServiceGuid = serviceWrapper.ServiceGuid,
    };
  }
}