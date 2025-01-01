using JetBrains.Annotations;

namespace Oma.WndwCtrl.Configuration.Model;

[Serializable]
public class ConfigurationMetadata
{
  [UsedImplicitly]
  public string FileName { get; set; } = string.Empty;

  [UsedImplicitly]
  public DateTime LastModified { get; set; } = DateTime.UtcNow;
}