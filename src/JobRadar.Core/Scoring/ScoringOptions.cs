namespace JobRadar.Core.Scoring;

public sealed class ScoringOptions
{
    public PlusScores Plus { get; init; } = new();
    public MinusScores Minus { get; init; } = new();

    public sealed class PlusScores
    {
        public int JuniorInTitle { get; init; } = 3;
        public int TraineeInTitle { get; init; } = 3;
        public int SkillCsharp { get; init; } = 2;
        public int SkillDotnet { get; init; } = 2;
        public int SkillJava { get; init; } = 2;
        public int SkillSql { get; init; } = 1;
        public int SkillDocker { get; init; } = 1;
        public int SkillGit { get; init; } = 1;
        public int SkillLinux { get; init; } = 1;
        public int HomeOffice { get; init; } = 2;
        public int SalaryAboveTarget { get; init; } = 2;
        public int SlovakOrCzech { get; init; } = 3;
    }

    public sealed class MinusScores
    {
        public int SupportKeywords { get; init; } = -2;
        public int GermanNativeRequired { get; init; } = -1;
        public int FrenchRequired { get; init; } = -2;
        public int SalaryBelowTarget { get; init; } = -1;
    }
}
