using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

[PublicAPI]
public interface IMessage
{
  string Topic { get; }

  string Type { get; }

  string Name { get; }

  string? ComponentName { get; }
}