using JetBrains.Annotations;
using Oma.WndwCtrl.Abstractions.Messaging.Model;

namespace Oma.WndwCtrl.Scheduling.Interfaces;

[PublicAPI]
public interface IJobList
{
  string Name { get; }

  IAsyncEnumerable<Job> GetJobsToExecuteAsync(
    DateTime? referenceTime,
    CancellationToken cancelToken = default
  );

  Task<IEnumerable<Job>> GetScheduledJobsAsync(CancellationToken cancelToken = default);

  Task StoreAsync(CancellationToken cancelToken = default);

  Task<int> MergeJobsAsync(IList<Job> jobsFromConfig, CancellationToken cancelToken = default);

  Task<int> FlagAllToFireAsync(CancellationToken cancelToken = default);
}