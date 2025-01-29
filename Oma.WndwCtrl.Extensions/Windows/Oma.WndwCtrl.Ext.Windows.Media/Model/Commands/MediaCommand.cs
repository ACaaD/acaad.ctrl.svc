using JetBrains.Annotations;
using Oma.WndwCtrl.Core.Model.Commands;

namespace Oma.WndwCtrl.Ext.Windows.Media.Model.Commands;

[PublicAPI]
public class MediaCommand : BaseCommand
{
  public override string Category => "Media";

  public string Action { get; init; } = string.Empty;
}