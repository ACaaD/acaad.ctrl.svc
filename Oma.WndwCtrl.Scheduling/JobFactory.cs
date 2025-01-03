using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Scheduling.Interfaces;
using Oma.WndwCtrl.Scheduling.Model;

namespace Oma.WndwCtrl.Scheduling;

public class JobFactory : IJobFactory
{
  public bool TryGetNext(Job current, [NotNullWhen(returnValue: true)] out Job? next)
  {
    next = current.Trigger switch
    {
      var _ => null,
    };

    next?.SetPrevious(current);

    return next is not null;
  }

  public Job CreateJob(DateTime referenceDate, ITrigger trigger)
  {
  }
}