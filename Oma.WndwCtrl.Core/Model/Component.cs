using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Button), "button")]
[JsonDerivedType(typeof(Sensor), "sensor")]
[JsonDerivedType(typeof(Switch), "switch")]
public abstract class Component : IComponent, IHasTriggers
{
  public string Name { get; set; } = string.Empty;
  public IEnumerable<ITrigger> Triggers { get; init; } = [];
}