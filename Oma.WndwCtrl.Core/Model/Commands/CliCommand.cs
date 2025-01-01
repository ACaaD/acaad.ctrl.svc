using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Oma.WndwCtrl.Core.Model.Commands;

[Serializable]
public class CliCommand : BaseCommand
{
  [JsonRequired]
  public string FileName { get; set; } = null!;

  [PublicAPI]
  public string? WorkingDirectory { get; set; }

  public string? Arguments { get; set; }

  public override string ToString()
  {
    return $"CLI: {FileName} {Arguments}";
  }
}