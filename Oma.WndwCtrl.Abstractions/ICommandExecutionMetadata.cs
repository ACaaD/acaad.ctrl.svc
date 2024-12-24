using LanguageExt;

namespace Oma.WndwCtrl.Abstractions;

public interface ICommandExecutionMetadata
{
    Option<TimeSpan> ExecutionDuration { get; }
    
    Option<int> ExecutedRetries { get; }
}