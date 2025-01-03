using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Core.Model.Triggers;

[PublicAPI]
public record EventTrigger : BaseTrigger
{
  public string? Topic { get; init; }
  public string? Type { get; init; }
  public string? Name { get; init; }
  public string? ComponentName { get; init; }
  public string? Match { get; init; }

  public bool Handles(IMessage message)
  {
    if (Topic is not null && message.Topic != Topic)
    {
      return false;
    }

    if (Type is not null && message.Type != Type)
    {
      return false;
    }

    if (Name is not null && message.Name != Name)
    {
      return false;
    }

    if (ComponentName is not null && message.ComponentName != ComponentName)
    {
      return false;
    }

    if (Match is not null)
    {
      throw new NotImplementedException("Matching events is not yet implemented.");
    }

    return true;
  }
}