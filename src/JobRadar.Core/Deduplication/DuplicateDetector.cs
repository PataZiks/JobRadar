using JobRadar.Core.Models;

namespace JobRadar.Core.Deduplication;

public sealed class DuplicateDetector
{
    public IReadOnlyList<JobPost> Deduplicate(IReadOnlyList<JobPost> jobs)
    {
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenTitleCompany = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<JobPost>(jobs.Count);

        foreach (var job in jobs)
        {
            var titleCompanyKey = $"{Normalize(job.Title)}|{Normalize(job.Company)}";

            // Keep only if both the ID and the title+company pair are new
            if (seenIds.Add(job.Id) && seenTitleCompany.Add(titleCompanyKey))
                result.Add(job);
        }

        return result;
    }

    private static string Normalize(string s) =>
        s.ToLowerInvariant().Trim();
}
