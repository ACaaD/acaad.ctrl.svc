using Oma.WndwCtrl.Abstractions.Messaging.Model;
using Oma.WndwCtrl.Scheduling.Model;

namespace Oma.WndwCtrl.Scheduling.Interfaces;

public interface IJobList
{
  string Name { get; }

  IAsyncEnumerable<Job> GetJobsToExecuteAsync(
    DateTime? referenceTime,
    CancellationToken cancelToken = default
  );

  Task<IEnumerable<Job>> GetScheduledJobsAsync();

  Task StoreAsync(CancellationToken cancelToken);

  Task<int> LoadAsync(DateTime referenceDate, CancellationToken cancelToken);

  Task<int> FlagAllToFireAsync(CancellationToken cancelToken);
}