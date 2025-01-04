namespace Oma.WndwCtrl.Abstractions;

public interface ITrigger
{
  Guid UniqueIdentifier { get; }

  string? FriendlyName { get; set; }
}