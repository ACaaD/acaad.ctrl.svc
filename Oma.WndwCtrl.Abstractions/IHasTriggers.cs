using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions;

[PublicAPI]
public interface IHasTriggers
{
  [JsonIgnore]
  IEnumerable<ITrigger> Triggers { get; }
}