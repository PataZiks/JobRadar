namespace JobRadar.Core.Filtering;

public sealed class FilterOptions
{
    public ExcludeOptions Exclude { get; init; } = new();
    public IncludeOptions Include { get; init; } = new();

    public sealed class ExcludeOptions
    {
        public List<string> TitleKeywords { get; init; } = [];
        public List<string> DescriptionKeywords { get; init; } = [];
    }

    public sealed class IncludeOptions
    {
        public List<string> TitleKeywords { get; init; } = [];
    }
}
