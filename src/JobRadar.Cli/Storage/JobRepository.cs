using Dapper;
using JobRadar.Core.Models;
using Microsoft.Data.Sqlite;

namespace JobRadar.Cli.Storage;

public sealed class JobRepository : IDisposable
{
    private readonly SqliteConnection _db;

    public JobRepository(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _db = new SqliteConnection($"Data Source={dbPath}");
        _db.Open();
        InitializeSchema();
    }

    private void InitializeSchema()
    {
        _db.Execute("""
            CREATE TABLE IF NOT EXISTS seen_jobs (
                id          TEXT NOT NULL PRIMARY KEY,
                title       TEXT NOT NULL,
                company     TEXT NOT NULL,
                location    TEXT NOT NULL,
                link        TEXT NOT NULL,
                source      TEXT NOT NULL,
                first_seen  TEXT NOT NULL DEFAULT (datetime('now'))
            );
            """);
    }

    /// <summary>
    /// Filters the list to only jobs not yet in the database, marks them as new,
    /// then persists them so the next run won't show them again.
    /// </summary>
    public IReadOnlyList<JobPost> FilterAndMarkNew(IReadOnlyList<JobPost> jobs)
    {
        var newJobs = jobs.Where(j => !IsSeen(j.Id)).ToList();

        foreach (var job in newJobs)
        {
            job.IsNew = true;
            MarkSeen(job);
        }

        foreach (var job in jobs.Except(newJobs))
            job.IsNew = false;

        return newJobs;
    }

    public bool IsSeen(string jobId) =>
        _db.ExecuteScalar<int>(
            "SELECT COUNT(1) FROM seen_jobs WHERE id = @Id",
            new { Id = jobId }) > 0;

    private void MarkSeen(JobPost job) =>
        _db.Execute(
            """
            INSERT OR IGNORE INTO seen_jobs (id, title, company, location, link, source)
            VALUES (@Id, @Title, @Company, @Location, @Link, @Source)
            """,
            new
            {
                job.Id, job.Title, job.Company,
                job.Location, job.Link, job.Source
            });

    public int CountSeen() =>
        _db.ExecuteScalar<int>("SELECT COUNT(1) FROM seen_jobs");

    public void Dispose() => _db.Dispose();
}
