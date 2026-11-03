using JobRadar.Core.Deduplication;
using JobRadar.Core.Filtering;
using JobRadar.Core.Models;
using JobRadar.Core.Scoring;
using JobRadar.Core.Sources;

namespace JobRadar.Cli;

public sealed class JobRunner
{
    private readonly IJobSource _source;
    private readonly IJobFilter _filter;
    private readonly IJobScorer _scorer;
    private readonly DuplicateDetector _deduplicator;
    private readonly JobRunnerOptions _options;

    public JobRunner(
        IJobSource source,
        IJobFilter filter,
        IJobScorer scorer,
        DuplicateDetector deduplicator,
        JobRunnerOptions options)
    {
        _source = source;
        _filter = filter;
        _scorer = scorer;
        _deduplicator = deduplicator;
        _options = options;
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        var all = await _source.FetchAsync(cancellationToken);

        // Filter
        var passed = new List<JobPost>();
        foreach (var job in all)
        {
            var result = _filter.Evaluate(job);
            if (result.ShouldKeep)
                passed.Add(job);
        }

        // Score
        foreach (var job in passed)
            _scorer.Score(job);

        // Deduplicate and sort
        var unique = _deduplicator.Deduplicate(passed);
        var sorted = unique.OrderByDescending(j => j.Score).ToList();

        if (_options.DryRun)
        {
            Console.WriteLine($"[dry-run] Would process {sorted.Count} job(s). No output written.");
            return;
        }

        // Output
        var line = new string('─', 72);
        var label = _options.ShowAll ? "all" : "new";
        Console.WriteLine($"JobRadar  |  Source: {_source.Name}  |  {all.Count} fetched  →  {sorted.Count} {label} after filter/dedup");
        Console.WriteLine(new string('═', 72));

        foreach (var job in sorted)
        {
            var title = string.IsNullOrEmpty(job.Warning)
                ? job.Title
                : $"{job.Title}  [{job.Warning}]";

            Console.WriteLine($"  Score {job.Score:+0;-0;+0}  |  {title}");
            Console.WriteLine($"  {job.Company}  ·  {job.Location}  ·  {job.Type}");

            if (!string.IsNullOrWhiteSpace(job.Salary))
                Console.WriteLine($"  Gehalt: {job.Salary}");

            if (!string.IsNullOrWhiteSpace(job.ScoreReason))
                Console.WriteLine($"  Score-Begründung: {job.ScoreReason}");

            var snippet = job.Snippet.Length > 120
                ? job.Snippet[..120] + "…"
                : job.Snippet;
            Console.WriteLine($"  {snippet}");
            Console.WriteLine($"  {job.Link}");
            Console.WriteLine(line);
        }
    }
}
