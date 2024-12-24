using LanguageExt;

namespace Oma.WndwCtrl.Abstractions.Model;

public record CommandOutcome : ICommandExecutionMetadata
{
    public string OutcomeRaw { get; set; } = string.Empty;
    
    public Option<TimeSpan> ExecutionDuration { get; set; } = Option<TimeSpan>.None;
    public Option<int> ExecutedRetries { get; set; } = Option<int>.None;
}