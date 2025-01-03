using LanguageExt;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;

namespace Oma.WndwCtrl.Scheduling.Interfaces;

public interface IJobFactory
{
  bool Handles(ISchedulableTrigger trigger);

  Option<Job> CreateJob(DateTime referenceDate, ISchedulableTrigger trigger);

  Option<Job> GetNext(Job current);
}

public interface IJobFactory<TTrigger> : IJobFactory
  where TTrigger : ISchedulableTrigger
{
  bool IJobFactory.Handles(ISchedulableTrigger trigger) => trigger is TTrigger;
}