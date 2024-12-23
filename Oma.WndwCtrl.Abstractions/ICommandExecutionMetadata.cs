namespace Oma.WndwCtrl.Abstractions;

public interface ICommandExecutionMetadata
{
    TimeSpan ExecutionDuration { get; }
    
    int ExecutedRetries { get; }
}