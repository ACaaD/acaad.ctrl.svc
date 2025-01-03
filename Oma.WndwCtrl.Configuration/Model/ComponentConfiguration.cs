using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Model;

namespace Oma.WndwCtrl.Configuration.Model;

public class ComponentConfiguration : IHasTriggers
{
  [JsonRequired]
  [JsonInclude]
  public IReadOnlyDictionary<string, Component> Components { get; init; } =
    new Dictionary<string, Component>();

  [JsonPropertyName("__meta")]
  [JsonPropertyOrder(int.MaxValue)]
  [JsonInclude]
  public ConfigurationMetadata Metadata { get; init; } = new();

  [JsonIgnore]
  public IEnumerable<ITrigger> Triggers => Components
    .SelectMany(component => component.Value.Triggers);
}