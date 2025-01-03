using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Abstractions.Messaging.Model;

public abstract record Event : IMessage
{
  public string Topic => "Events";
}