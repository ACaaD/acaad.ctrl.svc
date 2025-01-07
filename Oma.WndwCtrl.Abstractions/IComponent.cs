using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions;

[PublicAPI]
public interface IComponent : IHasCommands
{
  string Name { get; set; }

  string Type { get; }
}