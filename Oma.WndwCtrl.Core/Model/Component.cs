using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Core.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Button), "button")]
[JsonDerivedType(typeof(Sensor), "sensor")]
[JsonDerivedType(typeof(Switch), "switch")]
public abstract class Component
{
}