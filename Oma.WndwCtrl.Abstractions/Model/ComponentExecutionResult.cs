using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Abstractions.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ComponentExecutionResult
{
  Succeeded,
  PartiallySucceeded,
  Failed,
}