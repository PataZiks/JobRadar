using System.Text.Json;
using JobRadar.Core.Models;
using JobRadar.Core.Sources;

namespace JobRadar.Cli.Sources;

public sealed class FakeJobSource : IJobSource
{
    private readonly string _fixturePath;

    public string Name => "Fake";

    public FakeJobSource(string fixturePath)
    {
        _fixturePath = fixturePath;
    }

    public async Task<IReadOnlyList<JobPost>> FetchAsync(CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(_fixturePath);

        var response = await JsonSerializer.DeserializeAsync<JoobleResponse>(
            stream,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            cancellationToken);

        return response?.Jobs
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
            .ToList()
            ?? [];
    }

    // DTOs for the Jooble JSON structure – private, only used here
    private sealed class JoobleResponse
    {
        public int TotalCount { get; init; }
        public List<JoobleJob> Jobs { get; init; } = [];
    }

    private sealed class JoobleJob
    {
        public string Id      { get; init; } = string.Empty;
        public string Title   { get; init; } = string.Empty;
        public string Company { get; init; } = string.Empty;
        public string Location{ get; init; } = string.Empty;
        public string Snippet { get; init; } = string.Empty;
        public string Salary  { get; init; } = string.Empty;
        public string Source  { get; init; } = string.Empty;
        public string Type    { get; init; } = string.Empty;
        public string Link    { get; init; } = string.Empty;
        public string Updated { get; init; } = string.Empty;
    }
}
