using System.Collections.Concurrent;
using System.Threading.Channels;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Model;

namespace Oma.WndwCtrl.Messaging.Bus;

public class MessageBusState([FromKeyedServices(ServiceKeys.MessageBus)] Channel<IMessage> channel)
{
  private readonly ConcurrentDictionary<string, Channel<IMessage>> _activeChannels = new();

  public Channel<IMessage> Queue { get; } = channel;

  public IEnumerable<Channel<IMessage>> ActiveChannels => _activeChannels.Values;

  public Channel<IMessage> Add(string channelName, Channel<IMessage> channel) =>
    _activeChannels.TryAdd(channelName, channel)
      ? channel
      : throw new InvalidOperationException($"Channel {channelName} already exists");

  public Option<Channel<IMessage>> TryRemove(string channelName) =>
    _activeChannels.TryRemove(channelName, out Channel<IMessage>? channel)
      ? Option<Channel<IMessage>>.Some(channel)
      : Option<Channel<IMessage>>.None;
}