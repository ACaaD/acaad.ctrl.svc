using System.Text.Json.Serialization;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Core.Interfaces;

namespace Oma.WndwCtrl.Core.Model;

/// <summary>
/// A read+write control indicating on/off.
/// Can define a GET endpoint to query the _current_ state (ad-hoc execution)
/// Additionally, a POST:/on and POST:/off endpoint is hosted (potentially flip as well)
/// </summary>
public class Switch : Component, IStateQueryable
{
  [JsonIgnore]
  public override string Type => "switch";

  [JsonInclude]
  [JsonRequired]
  public ICommand OnCommand { get; internal set; } = null!;

  [JsonInclude]
  [JsonRequired]
  public ICommand OffCommand { get; internal set; } = null!;

  [JsonIgnore]
  public override IEnumerable<ICommand> Commands => [QueryCommand,];

  [JsonInclude]
  [JsonRequired]
  public ICommand QueryCommand { get; internal set; } = null!;
}