namespace Oma.WndwCtrl.Abstractions;

public interface ICommand
{
  int Retries { get; }

  TimeSpan Timeout { get; }

  IEnumerable<ITransformation> Transformations { get; }
}