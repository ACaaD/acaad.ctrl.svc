using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

public class DummyMessage : IMessage
{
  public string Topic => nameof(DummyMessage);
}