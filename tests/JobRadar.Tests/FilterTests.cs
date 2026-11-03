using JobRadar.Core.Filtering;
using JobRadar.Core.Models;

namespace JobRadar.Tests;

public sealed class FilterTests
{
    private static JobFilter BuildFilter(
        string[]? excludeTitle = null,
        string[]? excludeDesc = null,
        string[]? includeTitle = null) =>
        new(new FilterOptions
        {
            Exclude = new FilterOptions.ExcludeOptions
            {
                TitleKeywords = [.. (excludeTitle ?? ["senior", "lead", "praktikum"])],
                DescriptionKeywords = [.. (excludeDesc ?? ["mehrjährige erfahrung"])]
            },
            Include = new FilterOptions.IncludeOptions
            {
                TitleKeywords = [.. (includeTitle ?? ["junior", "trainee"])]
            }
        });

    [Fact]
    public void Junior_title_passes_filter()
    {
        var filter = BuildFilter();
        var job = new JobPost { Title = "Junior .NET Developer", Snippet = "Einsteigerstelle." };

        var result = filter.Evaluate(job);

        Assert.True(result.ShouldKeep);
    }

    [Fact]
    public void Senior_title_is_excluded()
    {
        var filter = BuildFilter();
        var job = new JobPost { Title = "Senior Software Engineer", Snippet = "5+ Jahre Erfahrung." };

        var result = filter.Evaluate(job);

        Assert.False(result.ShouldKeep);
        Assert.Contains("senior", result.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Praktikum_type_is_excluded()
    {
        var filter = BuildFilter();
        var job = new JobPost { Title = "Softwareentwicklung", Type = "Praktikum", Snippet = "" };

        var result = filter.Evaluate(job);

        Assert.False(result.ShouldKeep);
    }

    [Fact]
    public void Excluded_description_keyword_is_rejected()
    {
        var filter = BuildFilter();
        var job = new JobPost { Title = "IT-Stelle", Snippet = "Wir erwarten mehrjährige Erfahrung in .NET." };

        var result = filter.Evaluate(job);

        Assert.False(result.ShouldKeep);
    }

    [Fact]
    public void Experience_warning_is_added_but_job_is_kept()
    {
        var filter = BuildFilter();
        var job = new JobPost { Title = "Junior Dev", Snippet = "1-2 jahre Berufserfahrung von Vorteil." };

        var result = filter.Evaluate(job);

        Assert.True(result.ShouldKeep);
        Assert.Contains("⚠️", job.Warning);
    }

    [Fact]
    public void Unknown_job_without_keywords_is_kept_by_default()
    {
        var filter = BuildFilter();
        var job = new JobPost { Title = "IT-Techniker", Snippet = "Allgemeine IT-Stelle ohne klare Einordnung." };

        var result = filter.Evaluate(job);

        Assert.True(result.ShouldKeep);
    }
}
