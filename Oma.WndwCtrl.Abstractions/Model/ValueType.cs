using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Abstractions.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValueType
{
  Boolean,
  String,
  Long,
  Decimal,
}