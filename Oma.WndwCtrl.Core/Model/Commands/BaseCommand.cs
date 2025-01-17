using System.Text.Json.Serialization;
using LanguageExt;
using Oma.WndwCtrl.Abstractions;

namespace Oma.WndwCtrl.Core.Model.Commands;

[Serializable]
public abstract class BaseCommand : ICommand
{
  public TimeSpan WaitOnComplete { get; set; } = TimeSpan.Zero;
  public int Retries { get; set; }

  public TimeSpan Timeout { get; set; }

  [JsonIgnore]
  public abstract string Category { get; }

  public IEnumerable<ITransformation> Transformations { get; set; } = new List<ITransformation>();

  [JsonIgnore]
  public Option<IComponent> Component { get; set; }
}