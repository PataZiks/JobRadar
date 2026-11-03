using JobRadar.Core.Deduplication;
using JobRadar.Core.Models;

namespace JobRadar.Tests;

public sealed class DuplicateDetectorTests
{
    private readonly DuplicateDetector _detector = new();

    [Fact]
    public void Unique_jobs_are_all_kept()
    {
        var jobs = new[]
        {
            new JobPost { Id = "1", Title = "Junior Dev", Company = "Firma A" },
            new JobPost { Id = "2", Title = "Senior Dev", Company = "Firma B" },
        };

        var result = _detector.Deduplicate(jobs);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Same_id_is_deduplicated()
    {
        var jobs = new[]
        {
            new JobPost { Id = "abc", Title = "Junior Dev", Company = "Firma A" },
            new JobPost { Id = "abc", Title = "Junior Dev", Company = "Firma A" },
        };

        var result = _detector.Deduplicate(jobs);

        Assert.Single(result);
    }

    [Fact]
    public void Same_title_and_company_via_different_agency_is_deduplicated()
    {
        var jobs = new[]
        {
            new JobPost { Id = "x1", Title = "Junior .NET Developer", Company = "Hays" },
            new JobPost { Id = "x2", Title = "Junior .NET Developer", Company = "Hays" },
        };

        var result = _detector.Deduplicate(jobs);

        Assert.Single(result);
    }

    [Fact]
    public void Same_title_different_company_is_kept()
    {
        var jobs = new[]
        {
            new JobPost { Id = "1", Title = "Junior Developer", Company = "Firma A" },
            new JobPost { Id = "2", Title = "Junior Developer", Company = "Firma B" },
        };

        var result = _detector.Deduplicate(jobs);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void First_occurrence_is_kept()
    {
        var jobs = new[]
        {
            new JobPost { Id = "original", Title = "Junior Dev", Company = "X" },
            new JobPost { Id = "duplicate", Title = "Junior Dev", Company = "X" },
        };

        var result = _detector.Deduplicate(jobs);

        Assert.Equal("original", result[0].Id);
    }
}
