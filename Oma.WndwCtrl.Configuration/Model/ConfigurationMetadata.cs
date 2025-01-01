namespace Oma.WndwCtrl.Configuration.Model;

public class ConfigurationMetadata
{
  public string FileName { get; set; } = string.Empty;

  public DateTime LastModified { get; set; } = DateTime.UtcNow;
}