using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Abstractions.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueType
{
  String = 0,
  Boolean = 1,
  Long = 2,
  Decimal = 3,
}