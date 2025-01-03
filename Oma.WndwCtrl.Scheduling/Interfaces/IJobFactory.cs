using System.Diagnostics.CodeAnalysis;
using Oma.WndwCtrl.Abstractions;
using Oma.WndwCtrl.Abstractions.Messaging.Model;

namespace Oma.WndwCtrl.Scheduling.Interfaces;

public interface IJobFactory
{
  Job CreateJob(DateTime referenceDate, ITrigger trigger);

  bool TryGetNext(Job current, [NotNullWhen(returnValue: true)] out Job? next);
}