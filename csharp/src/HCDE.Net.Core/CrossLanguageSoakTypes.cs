namespace HCDE.Net.Core;

public enum CrossLanguageSoakStatus
{
    Skipped,
    Passed,
    Failed,
}

public readonly struct CrossLanguageSoakResult
{
    public CrossLanguageSoakResult(CrossLanguageSoakStatus status, string output, string? skipReason = null)
    {
        Status = status;
        Output = output;
        SkipReason = skipReason;
    }

    public CrossLanguageSoakStatus Status { get; }
    public string Output { get; }
    public string? SkipReason { get; }
}
