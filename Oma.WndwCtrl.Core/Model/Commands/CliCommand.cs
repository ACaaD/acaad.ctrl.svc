using System.Text.Json.Serialization;

namespace Oma.WndwCtrl.Core.Model.Commands;

public class CliCommand : BaseCommand
{
  [JsonRequired]
  public string FileName { get; set; } = null!;

  public string? WorkingDirectory { get; set; }

  public string? Arguments { get; set; }

  public override string ToString()
  {
    return $"CLI: {FileName} {Arguments}";
  }
}