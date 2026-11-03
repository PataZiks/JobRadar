using JobRadar.Cli;
using JobRadar.Cli.Sources;
using JobRadar.Core.Deduplication;
using JobRadar.Core.Filtering;
using JobRadar.Core.Scoring;
using JobRadar.Core.Sources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        var filterOptions = cfg.GetSection("JobRadar:Filter").Get<FilterOptions>() ?? new FilterOptions();
        var scoringOptions = cfg.GetSection("JobRadar:Scoring").Get<ScoringOptions>() ?? new ScoringOptions();
        var salaryTarget = cfg.GetValue<decimal>("JobRadar:Salary:TargetAnnual", 42_000);

        var fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "jooble_sample_response.json");

        services.AddSingleton<IJobSource>(new FakeJobSource(fixturePath));
        services.AddSingleton<IJobFilter>(new JobFilter(filterOptions));
        services.AddSingleton<IJobScorer>(new JobScorer(scoringOptions, salaryTarget));
        services.AddSingleton<DuplicateDetector>();
        services.AddSingleton<JobRunner>();
    })
    .Build();

await host.Services.GetRequiredService<JobRunner>().RunAsync();
