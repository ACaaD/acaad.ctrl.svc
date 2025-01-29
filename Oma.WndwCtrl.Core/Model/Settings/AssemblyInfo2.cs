using JetBrains.Annotations;

namespace Oma.WndwCtrl.Core.Model.Settings;

[UsedImplicitly]
[PublicAPI]
public record AssemblyInfo2
{
  public string AssemblyName { get; set; } = string.Empty;
}