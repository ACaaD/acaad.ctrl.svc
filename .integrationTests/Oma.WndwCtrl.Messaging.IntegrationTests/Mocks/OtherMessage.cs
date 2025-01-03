using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.IntegrationTests.Mocks;

public class OtherMessage : IMessage
{
  public string Topic => nameof(OtherMessage);
  public string Type => string.Empty;
  public string Name => string.Empty;
  public string ComponentName => string.Empty;
}