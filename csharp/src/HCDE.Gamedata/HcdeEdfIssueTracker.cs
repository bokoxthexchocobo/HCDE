namespace HCDE.Gamedata;

public sealed class HcdeEdfIssueSample
{
    public string Source { get; set; } = "";
    public string Context { get; set; } = "";
    public string Detail { get; set; } = "";
    public int Line { get; set; }
}

public static class HcdeEdfIssueTracker
{
    public const int MaxSamples = 20;

    public static bool TryRecordIssue(List<HcdeEdfIssueSample> samples, string context, string detail, int line)
    {
        if (samples.Count >= MaxSamples) return false;
        samples.Add(new HcdeEdfIssueSample { Context = context, Detail = detail, Line = line });
        return true;
    }
}
