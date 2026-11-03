using JobRadar.Cli.Sources;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "jooble_sample_response.json");
var source = new FakeJobSource(fixturePath);

var jobs = await source.FetchAsync();

Console.WriteLine($"JobRadar  |  Source: {source.Name}  |  {jobs.Count} job(s) found");
Console.WriteLine(new string('=', 72));

foreach (var job in jobs)
{
    Console.WriteLine($"  {job.Title}");
    Console.WriteLine($"  {job.Company}  |  {job.Location}  |  {job.Type}");

    if (!string.IsNullOrWhiteSpace(job.Salary))
        Console.WriteLine($"  Salary: {job.Salary}");

    var snippet = job.Snippet.Length > 100
        ? job.Snippet[..100] + "..."
        : job.Snippet;
    Console.WriteLine($"  {snippet}");
    Console.WriteLine($"  {job.Link}");
    Console.WriteLine(new string('-', 72));
}
