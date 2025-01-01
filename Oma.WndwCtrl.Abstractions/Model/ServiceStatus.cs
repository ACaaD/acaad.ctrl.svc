using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Abstractions.Model;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ServiceStatus
{
  Starting = 0,
  Running = 1,
  Stopping = 2,
  Stopped = 3,
  Crashed = 4,
}