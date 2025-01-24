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

  [JsonIgnore]
  public IReadOnlyDictionary<string, Component> ActiveComponents => Components.Where(kvp => kvp.Value.Active)
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

  [JsonPropertyName("__meta")]
  [JsonPropertyOrder(int.MaxValue)]
  [JsonInclude]
  public ConfigurationMetadata Metadata { get; init; } = new();

  [JsonIgnore]
  public IEnumerable<ITrigger> Triggers => ActiveComponents
    .SelectMany(component => component.Value.Triggers);
}