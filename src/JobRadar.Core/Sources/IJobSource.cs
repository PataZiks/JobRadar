using JobRadar.Core.Models;

namespace JobRadar.Core.Sources;

public interface IJobSource
{
    string Name { get; }
    Task<IReadOnlyList<JobPost>> FetchAsync(CancellationToken cancellationToken = default);
}
