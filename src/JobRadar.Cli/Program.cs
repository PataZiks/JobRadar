using System.Net.Http.Json;
using JobRadar.Cli.Storage;
using DotNetEnv;
using JobRadar.Cli;
using JobRadar.Cli.Infrastructure;
using JobRadar.Cli.Sources;
using JobRadar.Core.Deduplication;
using JobRadar.Core.Filtering;
using JobRadar.Core.Scoring;
using JobRadar.Core.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// Load .env from repo root
var envFile = FindEnvFile();
if (File.Exists(envFile))
    Env.Load(envFile);

// check-key: test the Jooble API key with exactly 1 request
if (args.Length > 0 && args[0] == "check-key")
{
    var apiKey = Environment.GetEnvironmentVariable("JOOBLE_API_KEY") ?? string.Empty;
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        Console.WriteLine("Error: JOOBLE_API_KEY is not set. Add it to .env or environment.");
        return 1;
    }

    var counterPath = Path.Combine(AppContext.BaseDirectory, "data", "request_counts.json");
    var counter = new RequestCounter(counterPath, limit: 500);

    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    try
    {
        var resp = await client.PostAsJsonAsync(
            $"https://jooble.org/api/{apiKey}",
            new { keywords = "junior", location = "Wien" });

        counter.Increment(apiKey);
        var used = counter.GetCount(apiKey);

        Console.WriteLine(resp.IsSuccessStatusCode
            ? $"Jooble key OK – {used}/500 used"
            : $"Jooble key error: HTTP {(int)resp.StatusCode} – {used}/500 used");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        return 1;
    }

    return 0;
}

// Parse CLI flags
var sourceOverride = args.FirstOrDefault(a => a.StartsWith("--source="))?.Split('=')[1]
    ?? args.SkipWhile(a => a != "--source").Skip(1).FirstOrDefault();
var dryRun  = args.Contains("--dry-run");
var showAll = args.Contains("--all");

var host = Host.CreateDefaultBuilder(args)
    .UseContentRoot(AppContext.BaseDirectory)
    .ConfigureAppConfiguration(cfg => cfg
        .AddJsonFile("appsettings.json", optional: false)
        .AddEnvironmentVariables())
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Warning);
    })
    .ConfigureServices((ctx, services) =>
    {
        var cfg = ctx.Configuration;

        var filterOptions  = cfg.GetSection("JobRadar:Filter").Get<FilterOptions>()   ?? new FilterOptions();
        var scoringOptions = cfg.GetSection("JobRadar:Scoring").Get<ScoringOptions>() ?? new ScoringOptions();
        var salaryTarget   = cfg.GetValue<decimal>("JobRadar:Salary:TargetAnnual", 42_000);
        var searchTerms    = cfg.GetSection("JobRadar:SearchTerms").Get<List<string>>() ?? [];
        var locations      = cfg.GetSection("JobRadar:Locations").Get<List<string>>()  ?? [];
        var joobleLimit    = cfg.GetValue<int>("JobRadar:ApiLimits:Jooble", 500);
        var configSource   = sourceOverride ?? cfg.GetValue<string>("JobRadar:Source", "fake");

        IJobSource source;
        if (configSource?.Equals("jooble", StringComparison.OrdinalIgnoreCase) == true)
        {
            var apiKey     = Environment.GetEnvironmentVariable("JOOBLE_API_KEY") ?? string.Empty;
            var counterPath = Path.Combine(AppContext.BaseDirectory, "data", "request_counts.json");
            var http       = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
            var counter    = new RequestCounter(counterPath, joobleLimit);
            var logger     = LoggerFactory.Create(b => b.AddConsole()).CreateLogger<JoobleJobSource>();
            source = new JoobleJobSource(http, counter, logger, apiKey, searchTerms, locations, joobleLimit);
        }
        else
        {
            var fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "jooble_sample_response.json");
            source = new FakeJobSource(fixturePath);
        }

        var dbPath = Path.Combine(AppContext.BaseDirectory, "data", "jobradar.db");

        services.AddSingleton(source);
        services.AddSingleton<IJobFilter>(new JobFilter(filterOptions));
        services.AddSingleton<IJobScorer>(new JobScorer(scoringOptions, salaryTarget));
        services.AddSingleton<DuplicateDetector>();
        services.AddSingleton(new JobRepository(dbPath));
        services.AddSingleton(new JobRunnerOptions(dryRun, showAll));
        services.AddSingleton<JobRunner>();
    })
    .Build();

await host.Services.GetRequiredService<JobRunner>().RunAsync();
return 0;

static string FindEnvFile()
{
    // Walk up from the binary location to find .env in the repo root
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir != null)
    {
        var candidate = Path.Combine(dir.FullName, ".env");
        if (File.Exists(candidate))
            return candidate;
        // Stop when we find a .git folder (repo root)
        if (Directory.Exists(Path.Combine(dir.FullName, ".git")))
            return candidate;
        dir = dir.Parent;
    }
    return Path.Combine(AppContext.BaseDirectory, ".env");
}
