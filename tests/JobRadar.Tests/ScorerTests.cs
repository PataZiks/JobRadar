using JobRadar.Core.Models;
using JobRadar.Core.Scoring;

namespace JobRadar.Tests;

public sealed class ScorerTests
{
    private static JobScorer BuildScorer(decimal salaryTarget = 42_000m) =>
        new(new ScoringOptions(), salaryTarget);

    [Fact]
    public void Junior_in_title_adds_plus_points()
    {
        var scorer = BuildScorer();
        var job = new JobPost { Title = "Junior .NET Developer", Snippet = "" };

        scorer.Score(job);

        Assert.True(job.Score > 0);
        Assert.Contains("Junior", job.ScoreReason);
    }

    [Fact]
    public void Csharp_skill_adds_points()
    {
        var scorer = BuildScorer();
        var job = new JobPost { Title = "Entwickler", Snippet = "Kenntnisse in C# und ASP.NET Core gewünscht." };

        scorer.Score(job);

        Assert.True(job.Score > 0);
        Assert.Contains("C#", job.ScoreReason);
    }

    [Fact]
    public void Home_office_adds_points()
    {
        var scorer = BuildScorer();
        var job = new JobPost { Title = "Developer", Snippet = "Home-Office möglich, flexible Arbeitszeiten." };

        scorer.Score(job);

        Assert.True(job.Score > 0);
        Assert.Contains("Home-Office", job.ScoreReason);
    }

    [Fact]
    public void Salary_above_target_adds_points()
    {
        var scorer = BuildScorer(salaryTarget: 42_000m);
        var job = new JobPost { Title = "Junior Dev", Snippet = "", Salary = "50000" };

        scorer.Score(job);

        Assert.Contains("Gehalt ≥ Ziel", job.ScoreReason);
    }

    [Fact]
    public void Salary_below_target_subtracts_points_and_adds_warning()
    {
        var scorer = BuildScorer(salaryTarget: 42_000m);
        var job = new JobPost { Title = "Junior Dev", Snippet = "", Salary = "30000" };

        scorer.Score(job);

        Assert.Contains("Gehalt unter Ziel", job.ScoreReason);
        Assert.Contains("💰", job.Warning);
    }

    [Fact]
    public void Empty_salary_is_neutral()
    {
        var scorer = BuildScorer();
        var job = new JobPost { Title = "Junior Dev", Snippet = "", Salary = "" };

        scorer.Score(job);

        Assert.DoesNotContain("Gehalt", job.ScoreReason);
    }

    [Fact]
    public void Support_keywords_subtract_points()
    {
        var scorer = BuildScorer();
        var job = new JobPost { Title = "IT-Stelle", Snippet = "1st level support und helpdesk Aufgaben." };

        scorer.Score(job);

        Assert.True(job.Score < 0);
    }

    [Fact]
    public void Slovak_language_adds_bonus()
    {
        var scorer = BuildScorer();
        var job = new JobPost { Title = "Junior Dev", Snippet = "Slowakisch oder Tschechisch ein großes Plus." };

        scorer.Score(job);

        Assert.Contains("SK/CZ", job.ScoreReason);
    }
}
