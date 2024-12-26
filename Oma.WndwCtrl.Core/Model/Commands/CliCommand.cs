using System.Text.Json.Serialization;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Model.Commands;

public class CliCommand : BaseCommand
{
    [JsonRequired]
    public string FileName { get; set; } = null!;

    public string? WorkingDirectory { get; set; }
    
    public string? Arguments { get; set; }
    
    public override string ToString() => $"CLI: {FileName} {Arguments}";
}
