using JobRadar.Core.Models;

namespace JobRadar.Cli.Reporting;

public static class HtmlReportWriter
{
    public static string Generate(IReadOnlyList<JobPost> jobs, string sourceName)
    {
        var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        var rows = string.Concat(jobs.Select(JobRow));

        return $$"""
            <!DOCTYPE html>
            <html lang="de">
            <head>
              <meta charset="utf-8">
              <meta name="viewport" content="width=device-width, initial-scale=1">
              <title>JobRadar – {{date}}</title>
              <style>
                body { font-family: system-ui, sans-serif; max-width: 900px; margin: 2rem auto; padding: 0 1rem; color: #1a1a1a; }
                h1   { font-size: 1.4rem; color: #2563eb; }
                .meta { color: #6b7280; font-size: 0.875rem; margin-bottom: 1.5rem; }
                .job  { border: 1px solid #e5e7eb; border-radius: 8px; padding: 1rem 1.25rem; margin-bottom: 1rem; }
                .job:hover { border-color: #93c5fd; }
                .title { font-size: 1.05rem; font-weight: 600; }
                .title a { color: #1d4ed8; text-decoration: none; }
                .title a:hover { text-decoration: underline; }
                .score { display: inline-block; background: #dbeafe; color: #1e40af; border-radius: 4px; padding: 1px 8px; font-size: 0.8rem; font-weight: 700; margin-left: 0.5rem; }
                .score.neg { background: #fee2e2; color: #991b1b; }
                .meta-row { color: #6b7280; font-size: 0.85rem; margin: 0.25rem 0; }
                .warning  { display: inline-block; background: #fef9c3; color: #854d0e; border-radius: 4px; padding: 1px 6px; font-size: 0.8rem; margin-left: 0.25rem; }
                .reason   { font-size: 0.8rem; color: #6b7280; margin-top: 0.4rem; }
                .snippet  { font-size: 0.875rem; color: #374151; margin-top: 0.5rem; }
              </style>
            </head>
            <body>
              <h1>JobRadar</h1>
              <p class="meta">{{date}} &nbsp;·&nbsp; Source: {{sourceName}} &nbsp;·&nbsp; {{jobs.Count}} new job(s)</p>
              {{rows}}
            </body>
            </html>
            """;
    }

    private static string JobRow(JobPost job)
    {
        var scoreClass = job.Score >= 0 ? "score" : "score neg";
        var warning = string.IsNullOrEmpty(job.Warning)
            ? ""
            : $"""<span class="warning">{Escape(job.Warning)}</span>""";
        var salary = string.IsNullOrEmpty(job.Salary)
            ? ""
            : $" &nbsp;·&nbsp; 💶 {Escape(job.Salary)}";

        return $"""
            <div class="job">
              <div class="title">
                <a href="{Escape(job.Link)}" target="_blank" rel="noopener">{Escape(job.Title)}</a>
                <span class="{scoreClass}">{job.Score:+0;-0;0}</span>
                {warning}
              </div>
              <div class="meta-row">{Escape(job.Company)} &nbsp;·&nbsp; {Escape(job.Location)} &nbsp;·&nbsp; {Escape(job.Type)}{salary}</div>
              <div class="reason">{Escape(job.ScoreReason)}</div>
              <div class="snippet">{Escape(job.Snippet)}</div>
            </div>
            """;
    }

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
}
