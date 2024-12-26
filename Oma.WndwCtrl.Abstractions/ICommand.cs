namespace Oma.WndwCtrl.Abstractions;

public interface ICommand
{
    int Retries { get; }
    
    TimeSpan Timeout { get; }
    
    IList<ITransformation> Transformations { get; }
}