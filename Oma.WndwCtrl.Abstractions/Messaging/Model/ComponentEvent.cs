namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

public abstract record ComponentEvent(IComponent Component) : Event
{
  public override string? ComponentName => Component.Name;
}