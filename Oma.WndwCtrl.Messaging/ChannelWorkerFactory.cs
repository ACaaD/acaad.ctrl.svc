using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Messaging.Model;

namespace Oma.WndwCtrl.Messaging;

public class ChannelWorkerFactory(IServiceProvider serviceProvider)
{
  public IChannelWorker CreateChannelWorker(object serviceKey)
  {
    WorkerMapping workerMapping = serviceProvider.GetRequiredKeyedService<WorkerMapping>(serviceKey);

    ChannelSettings channelSettings = serviceProvider.GetRequiredKeyedService<ChannelSettings>(serviceKey);

    Type workerType = typeof(ChannelWorker<,>).MakeGenericType(
      workerMapping.ConsumerType,
      workerMapping.MessageType
    );

    Channel<IMessage> channel = serviceProvider.GetRequiredKeyedService<Channel<IMessage>>(serviceKey);

    ChannelWorker channelWorker =
      (ChannelWorker)serviceProvider.GetRequiredKeyedService(workerType, serviceKey);

    channelWorker.Initialize(channelSettings, channel);

    return channelWorker;
  }
}