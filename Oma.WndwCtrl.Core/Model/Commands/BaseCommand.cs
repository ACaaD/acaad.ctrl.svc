using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Commands;

[Serializable]
public class BaseCommand : ICommand
{
  public int Retries { get; set; }

  public TimeSpan Timeout { get; set; }

  public IEnumerable<ITransformation> Transformations { get; set; } = new List<ITransformation>();
}