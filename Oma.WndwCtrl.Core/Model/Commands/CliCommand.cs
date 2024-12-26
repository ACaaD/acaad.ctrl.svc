using System.Text.Json.Serialization;
using Oma.WndwCtrl.Core.Model.Transformations;

namespace Oma.WndwCtrl.Core.Model.Commands;

public class CliCommand : BaseCommand
{
    public CliCommand(
        int retries, 
        TimeSpan timeout, 
        IList<BaseTransformation> transformations, 
        string fileName
        // ,
        // string? workingDirectory, 
        // string? arguments
    ) : base(retries, timeout, transformations)
    {
        FileName = fileName;
        // WorkingDirectory = workingDirectory;
        // Arguments = arguments;
    }

    [JsonRequired]
    public string FileName { get; set; } = null!;

    public string? WorkingDirectory { get; set; }

    public string? Arguments { get; set; }
    
    public override string ToString() => $"CLI: {FileName} {Arguments}";
}
