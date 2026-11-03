using System.Net.Http.Json;
using System.Text.Json;
using JobRadar.Cli.Infrastructure;
using JobRadar.Core.Models;
using JobRadar.Core.Sources;
using Microsoft.Extensions.Logging;

namespace JobRadar.Cli.Sources;

public sealed class JoobleJobSource : IJobSource
{
    private readonly HttpClient _http;
    private readonly RequestCounter _counter;
    private readonly ILogger<JoobleJobSource> _logger;
    private readonly string _apiKey;
    private readonly IReadOnlyList<string> _searchTerms;
    private readonly IReadOnlyList<string> _locations;
    private readonly int _limit;

    public string Name => "Jooble";

    public JoobleJobSource(
        HttpClient http,
        RequestCounter counter,
        ILogger<JoobleJobSource> logger,
        string apiKey,
        IReadOnlyList<string> searchTerms,
        IReadOnlyList<string> locations,
        int limit)
    {
        _http = http;
        _counter = counter;
        _logger = logger;
        _apiKey = apiKey;
        _searchTerms = searchTerms;
        _locations = locations;
        _limit = limit;
    }

    public async Task<IReadOnlyList<JobPost>> FetchAsync(CancellationToken cancellationToken = default)
    {
        if (_counter.IsLimitReached(_apiKey))
        {
            _logger.LogWarning("Jooble API limit reached ({Limit}). Skipping fetch.", _limit);
            return [];
        }

        if (_counter.IsNearLimit(_apiKey))
        {
            var used = _counter.GetCount(_apiKey);
            Console.WriteLine($"⚠️  Jooble-Key fast aufgebraucht – {used}/{_limit} Anfragen verwendet. Neuen Key anfragen!");
        }

        var all = new List<JobPost>();

        foreach (var term in _searchTerms)
        {
            foreach (var location in _locations)
            {
                var jobs = await FetchPageAsync(term, location, cancellationToken);
                all.AddRange(jobs);

                var used = _counter.GetCount(_apiKey);
                Console.WriteLine($"  Jooble: {used}/{_limit} used  (term='{term}', location='{location}', got {jobs.Count})");

                if (_counter.IsLimitReached(_apiKey))
                {
                    _logger.LogWarning("Jooble limit reached mid-run. Stopping further requests.");
                    return all;
                }
            }
        }

        return all;
    }

    private async Task<IReadOnlyList<JobPost>> FetchPageAsync(
        string term, string location, CancellationToken cancellationToken)
    {
        var requestBody = new { keywords = term, location };
        var url = $"https://jooble.org/api/{_apiKey}";

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync(url, requestBody, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Jooble request timed out (term={Term}, location={Location}).", term, location);
            return [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Jooble HTTP error (term={Term}, location={Location}).", term, location);
            return [];
        }

        _counter.Increment(_apiKey);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Jooble returned {Status} (term={Term}, location={Location}).",
                (int)response.StatusCode, term, location);
            return [];
        }

        JoobleResponse? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<JoobleResponse>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Jooble returned invalid JSON (term={Term}, location={Location}).", term, location);
            return [];
        }

        return result?.Jobs
            .Select(j => new JobPost
            {
                Id       = j.Id,
                Title    = j.Title,
                Company  = j.Company,
                Location = j.Location,
                Snippet  = j.Snippet,
                Salary   = j.Salary,
                Source   = j.Source,
                Type     = j.Type,
                Link     = j.Link,
                Updated  = j.Updated,
            })
            .ToList() ?? [];
    }

    private sealed class JoobleResponse
    {
        public int TotalCount { get; init; }
        public List<JoobleJob> Jobs { get; init; } = [];
    }

    private sealed class JoobleJob
    {
        public string Id       { get; init; } = string.Empty;
        public string Title    { get; init; } = string.Empty;
        public string Company  { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string Snippet  { get; init; } = string.Empty;
        public string Salary   { get; init; } = string.Empty;
        public string Source   { get; init; } = string.Empty;
        public string Type     { get; init; } = string.Empty;
        public string Link     { get; init; } = string.Empty;
        public string Updated  { get; init; } = string.Empty;
    }
}
