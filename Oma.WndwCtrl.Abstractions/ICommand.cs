using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions;

public interface ICommand
{
  [UsedImplicitly]
  int Retries { get; }

  [UsedImplicitly]
  TimeSpan Timeout { get; }

  IEnumerable<ITransformation> Transformations { get; }
}