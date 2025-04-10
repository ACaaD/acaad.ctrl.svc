using Oma.AirVentShaker.Api.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;

namespace Oma.AirVentShaker.Api.Model.Events;

public record GForceValueBatchEvent : Event
{
  public List<CurrentGForces> DataPoints { get; set; } = new();
  public override string Type { get; } = "AirVentShaker";
  public override string Name { get; } = nameof(GForceValueBatchEvent);
  public override string? ComponentName { get; } = null;
}