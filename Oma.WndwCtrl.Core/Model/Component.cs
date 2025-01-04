using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Button), "button")]
[JsonDerivedType(typeof(Sensor), "sensor")]
[JsonDerivedType(typeof(Switch), "switch")]
[PublicAPI]
public abstract class Component : IComponent, IHasTriggers
{
  public string Name { get; set; } = string.Empty;

  public abstract IEnumerable<ICommand> Commands { get; }
  public IEnumerable<ITrigger> Triggers { get; init; } = [];
}