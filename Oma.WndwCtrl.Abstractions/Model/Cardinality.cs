using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Abstractions.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Cardinality
{
  Single = 0,
  Multiple = 1,
}