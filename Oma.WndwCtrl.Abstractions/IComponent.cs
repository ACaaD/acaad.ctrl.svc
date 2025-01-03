using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions;

[PublicAPI]
public interface IComponent
{
  string Name { get; set; }
}