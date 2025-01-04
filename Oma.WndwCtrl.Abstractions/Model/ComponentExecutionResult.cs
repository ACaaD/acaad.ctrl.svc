using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
[Flags]
public enum ComponentExecutionResult
{
  Initial = 0,
  Succeeded = 1,
  Failed = 2,
  [UsedImplicitly] PartiallySucceeded = 3,
}