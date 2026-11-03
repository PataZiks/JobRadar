using JobRadar.Core.Models;

namespace JobRadar.Core.Scoring;

public interface IJobScorer
{
    void Score(JobPost job);
}
