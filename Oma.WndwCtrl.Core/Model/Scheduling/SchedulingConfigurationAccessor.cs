using JetBrains.Annotations;

namespace Oma.WndwCtrl.Core.Model.Scheduling;

[PublicAPI]
public record SchedulingConfigurationAccessor
{
  public SchedulingConfiguration Configuration { get; init; }
}