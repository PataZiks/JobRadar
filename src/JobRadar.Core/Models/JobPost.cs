namespace JobRadar.Core.Models;

public sealed class JobPost
{
    // Fields from the Jooble API response
    public string Id { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Company { get; init; } = string.Empty;
    public string Location { get; init; } = string.Empty;
    public string Snippet { get; init; } = string.Empty;
    public string Salary { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Link { get; init; } = string.Empty;
    public string Updated { get; init; } = string.Empty;

    // Filled by filter and scorer
    public int Score { get; set; }
    public string ScoreReason { get; set; } = string.Empty;
    public string Warning { get; set; } = string.Empty;
    public bool IsNew { get; set; } = true;
}
