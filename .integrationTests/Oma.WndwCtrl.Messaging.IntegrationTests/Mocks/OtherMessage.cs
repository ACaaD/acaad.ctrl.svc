using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

public class OtherMessage : IMessage
{
  public string Topic => nameof(OtherMessage);
}