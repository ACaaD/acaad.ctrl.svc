using JetBrains.Annotations;

namespace Oma.WndwCtrl.Abstractions;

[PublicAPI]
public interface ITrigger
{
  Guid UniqueIdentifier { get; }

  string? FriendlyName { get; set; }
}