using System.Text.Json.Serialization;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Configuration.Model;

public class ComponentConfiguration
{
  [JsonRequired]
  public IReadOnlyDictionary<string, Component> Components { get; set; } =
    new Dictionary<string, Component>();

  [JsonPropertyName("__meta")]
  [JsonPropertyOrder(int.MaxValue)]
  public ConfigurationMetadata Metadata { get; set; } = new();
}