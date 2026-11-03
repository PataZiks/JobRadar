using JobRadar.Core.Models;

namespace JobRadar.Core.Scoring;

public sealed class JobScorer : IJobScorer
{
    private readonly ScoringOptions _options;
    private readonly decimal _salaryTarget;

    public JobScorer(ScoringOptions options, decimal salaryTarget)
    {
        _options = options;
        _salaryTarget = salaryTarget;
    }

    public void Score(JobPost job)
    {
        var score = 0;
        var reasons = new List<string>();
        var title = job.Title.ToLowerInvariant();
        var text = (job.Title + " " + job.Snippet).ToLowerInvariant();

        Add(ref score, reasons, title.Contains("junior"),    _options.Plus.JuniorInTitle,  "Junior im Titel");
        Add(ref score, reasons, title.Contains("trainee"),   _options.Plus.TraineeInTitle, "Trainee im Titel");
        Add(ref score, reasons, text.Contains("c#"),         _options.Plus.SkillCsharp,    "C#");
        Add(ref score, reasons, text.Contains(".net"),       _options.Plus.SkillDotnet,    ".NET");
        Add(ref score, reasons, text.Contains("java"),       _options.Plus.SkillJava,      "Java");
        Add(ref score, reasons, text.Contains("sql"),        _options.Plus.SkillSql,       "SQL");
        Add(ref score, reasons, text.Contains("docker"),     _options.Plus.SkillDocker,    "Docker");
        Add(ref score, reasons, text.Contains("git"),        _options.Plus.SkillGit,       "Git");
        Add(ref score, reasons, text.Contains("linux"),      _options.Plus.SkillLinux,     "Linux");
        Add(ref score, reasons, text.Contains("home-office") || text.Contains("homeoffice") || text.Contains("remote"),
                                                             _options.Plus.HomeOffice,     "Home-Office");
        Add(ref score, reasons, text.Contains("slowakisch") || text.Contains("tschechisch") ||
                                text.Contains("slovenský") || text.Contains("česky"),
                                                             _options.Plus.SlovakOrCzech,  "SK/CZ Sprache");

        // Salary scoring
        var salaryScore = EvaluateSalary(job);
        if (salaryScore != 0)
        {
            score += salaryScore;
            if (salaryScore > 0)
                reasons.Add($"+{salaryScore} Gehalt ≥ Ziel");
            else
            {
                reasons.Add($"{salaryScore} Gehalt unter Ziel");
                if (string.IsNullOrEmpty(job.Warning))
                    job.Warning = "💰 unter Ziel";
                else
                    job.Warning += ", 💰 unter Ziel";
            }
        }

        // Minus points
        Add(ref score, reasons,
            text.Contains("1st level") || text.Contains("helpdesk") || text.Contains("hotline"),
            _options.Minus.SupportKeywords, "Support/Helpdesk");
        Add(ref score, reasons,
            text.Contains("deutsch muttersprache") || text.Contains("deutschkenntnisse muttersprache"),
            _options.Minus.GermanNativeRequired, "Deutsch Muttersprache");
        Add(ref score, reasons,
            text.Contains("französisch") || text.Contains("french"),
            _options.Minus.FrenchRequired, "Französisch Pflicht");

        // Recruiter marker (informational only, no score change)
        if (IsRecruiter(job.Company))
            job.Warning = string.IsNullOrEmpty(job.Warning)
                ? "📋 Personalvermittler"
                : job.Warning + ", 📋 Personalvermittler";

        job.Score = score;
        job.ScoreReason = string.Join(", ", reasons.Where(r => r.Length > 0));
    }

    private int EvaluateSalary(JobPost job)
    {
        if (string.IsNullOrWhiteSpace(job.Salary))
            return 0;

        // Extract the first number from the salary string
        var digits = new string(job.Salary.Where(c => char.IsDigit(c) || c == '.').ToArray());
        if (!decimal.TryParse(digits, out var amount) || amount <= 0)
            return 0;

        // Jooble sometimes provides monthly salary, detect by magnitude
        if (amount < 10_000)
            amount *= 14; // convert monthly (14-month Austrian convention) to annual

        return amount >= _salaryTarget
            ? _options.Plus.SalaryAboveTarget
            : _options.Minus.SalaryBelowTarget;
    }

    private static bool IsRecruiter(string company)
    {
        var known = new[] { "hays", "academic work", "techsearch", "isg", "manpower", "randstad", "adecco", "trenkwalder" };
        var lower = company.ToLowerInvariant();
        return known.Any(r => lower.Contains(r));
    }

    private static void Add(ref int score, List<string> reasons, bool condition, int points, string label)
    {
        if (!condition || points == 0) return;
        score += points;
        reasons.Add(points > 0 ? $"+{points} {label}" : $"{points} {label}");
    }
}
