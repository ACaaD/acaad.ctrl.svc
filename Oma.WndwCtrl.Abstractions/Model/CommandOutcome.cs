using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Model;

public record CommandOutcome : ICommandExecutionMetadata
{
    public string OutcomeRaw { get; set; }
    
    public TimeSpan ExecutionDuration { get; set; }
    public int ExecutedRetries { get; set; }
}