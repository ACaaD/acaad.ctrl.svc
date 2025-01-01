using System.Collections.Concurrent;
using System.Threading.Channels;
using LanguageExt;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;

namespace Oma.WndwCtrl.Messaging.Bus;

public class MessageBusState
{
  private readonly ConcurrentDictionary<string, Channel<IMessage>> _activeChannels = new();

  public Channel<IMessage> Queue { get; } = Channel.CreateUnbounded<IMessage>();

  public Channel<IMessage> Add(string channelName, Channel<IMessage> channel) =>
    _activeChannels.TryAdd(channelName, channel)
      ? channel
      : throw new InvalidOperationException($"Channel {channelName} already exists");

  public Option<Channel<IMessage>> TryRemove(string channelName) =>
    _activeChannels.TryRemove(channelName, out Channel<IMessage>? channel)
      ? Option<Channel<IMessage>>.Some(channel)
      : Option<Channel<IMessage>>.None;
}