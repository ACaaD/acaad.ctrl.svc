using System.Text.Json.Serialization;
using JetBrains.Annotations;
using Oma.WndwCtrl.Core.Model.Commands;

namespace Oma.WndwCtrl.Ext.Windows.Media.Model.Commands;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaAction
{
  Play,
  Pause,
  Stop,
  Next,
  Previous,
}

[PublicAPI]
public class MediaCommand : BaseCommand
{
  public override string Category => "Media";

  public MediaAction Action { get; init; }
}