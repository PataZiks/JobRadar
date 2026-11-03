namespace JobRadar.Core.Filtering;

public sealed record FilterResult(bool ShouldKeep, string Reason);
