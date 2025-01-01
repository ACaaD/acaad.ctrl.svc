using System.Diagnostics.CodeAnalysis;

namespace Oma.WndwCtrl.Messaging.Model;

public sealed record ChannelSettings
{
  [ExcludeFromCodeCoverage]
  public ushort Concurrency { get; init; } = 16;
}