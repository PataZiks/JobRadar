using JobRadar.Core.Models;

namespace JobRadar.Core.Filtering;

public sealed class JobFilter : IJobFilter
{
    private readonly FilterOptions _options;

    public JobFilter(FilterOptions options)
    {
        _options = options;
    }

    public FilterResult Evaluate(JobPost job)
    {
        var title = job.Title.ToLowerInvariant();
        var text = (job.Title + " " + job.Snippet).ToLowerInvariant();

        // Hard exclude: job type
        if (job.Type.Contains("praktikum", StringComparison.OrdinalIgnoreCase))
            return new FilterResult(false, $"Excluded: Praktikum (type={job.Type})");

        // Hard exclude: title keywords
        foreach (var keyword in _options.Exclude.TitleKeywords)
        {
            if (title.Contains(keyword.ToLowerInvariant()))
                return new FilterResult(false, $"Excluded: '{keyword}' in title");
        }

        // Hard exclude: description keywords
        foreach (var keyword in _options.Exclude.DescriptionKeywords)
        {
            if (text.Contains(keyword.ToLowerInvariant()))
                return new FilterResult(false, $"Excluded: '{keyword}' in description");
        }

        // Soft warning: experience requirement (keep but mark)
        if (ContainsExperienceWarning(text))
            job.Warning = "⚠️ 1–2 J. verlangt";

        return new FilterResult(true, "Passed filter");
    }

    private static bool ContainsExperienceWarning(string text) =>
        text.Contains("1-2 jahre") ||
        text.Contains("1–2 jahre") ||
        text.Contains("1 bis 2 jahre") ||
        text.Contains("ein bis zwei jahre");
}
