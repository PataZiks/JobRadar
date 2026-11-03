namespace JobRadar.Cli.Notifications;

public sealed class EmailOptions
{
    public string From { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    public string AppPassword { get; init; } = string.Empty;
    public bool SendOnlyIfNew { get; init; } = true;
}
