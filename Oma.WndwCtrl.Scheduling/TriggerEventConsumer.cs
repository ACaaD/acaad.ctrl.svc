using LanguageExt;
using Microsoft.Extensions.Logging;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Interfaces;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Configuration.Model;
using Oma.WndwCtrl.Core.Model;
using Oma.WndwCtrl.Core.Model.Triggers;
using Oma.WndwCtrl.Messaging;

namespace Oma.WndwCtrl.Scheduling;

public class TriggerEventConsumer(
  ILogger<TriggerEventConsumer> logger,
  ComponentConfigurationAccessor componentConfigurationAccessor,
  IMessageBusWriter messageBusWriter
) : IMessageConsumer<IMessage>
{
  private List<EventTrigger> _eventTriggers =
    componentConfigurationAccessor.Configuration.Triggers.OfType<EventTrigger>().ToList();

  // TODO: This is wrong because of event based triggers
  // This is a bit tricky, since it requires verifying _all_ events, potentially serializing them
  // to be able to apply the json path. This is performance critical.
  public bool IsSubscribedTo(IMessage message) =>
    // We can never analyze ComponentToRun events otherwise we might end up in endless loops.
    message is not ComponentToRunEvent;

  public async Task OnMessageAsync(IMessage message, CancellationToken cancelToken = default)
  {
    IEnumerable<ITrigger> triggersToSearch = _eventTriggers.Where(trigger => trigger.Handles(message));

    Job? job = null;

    if (message is ScheduledEvent scheduledEvent)
    {
      job = scheduledEvent.Job;
      triggersToSearch = triggersToSearch.Concat([job.Trigger,]);
    }

    foreach (Component component in triggersToSearch
               .Select(componentConfigurationAccessor.FindComponentByTrigger).Somes())
    {
      IMessage msg = new ComponentToRunEvent(component, job);
      await messageBusWriter.SendAsync(msg, cancelToken);
    }
  }

  public Task OnExceptionAsync(IMessage message, Exception exception, CancellationToken cancelToken = default)
  {
    logger.LogError(exception, "An unexpected error occurred processing message {message}.", message);
    return Task.CompletedTask;
  }
}