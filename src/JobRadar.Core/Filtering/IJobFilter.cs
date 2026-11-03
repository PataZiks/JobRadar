using JobRadar.Core.Models;

namespace JobRadar.Core.Filtering;

public interface IJobFilter
{
    FilterResult Evaluate(JobPost job);
}
