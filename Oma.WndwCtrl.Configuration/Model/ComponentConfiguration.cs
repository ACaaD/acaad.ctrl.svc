using System.Text.Json.Serialization;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Configuration.Model;

public class ComponentConfiguration
{
  [JsonRequired]
  public Dictionary<string, Component> Components { get; set; } = new();

  [JsonPropertyName("__meta")]
  [JsonPropertyOrder(int.MaxValue)]
  public ConfigurationMetadata Metadata { get; set; } = new();
}