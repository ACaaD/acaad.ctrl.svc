using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

public class DummyMessage : IMessage
{
  public string Topic => nameof(DummyMessage);
  public string Type { get; } = string.Empty;
  public string Name { get; } = string.Empty;
  public string? ComponentName { get; } = string.Empty;
}