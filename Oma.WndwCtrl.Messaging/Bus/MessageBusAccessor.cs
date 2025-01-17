using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.Bus;

[UsedImplicitly]
public record MessageBusAccessor
{
  public virtual IMessageBus? MessageBus { get; set; }
}