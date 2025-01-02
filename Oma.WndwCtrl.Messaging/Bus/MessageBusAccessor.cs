using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.Bus;

public record MessageBusAccessor
{
  public IMessageBus MessageBus { get; set; }
}