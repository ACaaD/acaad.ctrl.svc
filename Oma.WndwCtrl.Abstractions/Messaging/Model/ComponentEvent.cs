using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

[PublicAPI]
public abstract record ComponentEvent(IComponent Component) : Event
{
  public override string ComponentName => Component.Name;
}