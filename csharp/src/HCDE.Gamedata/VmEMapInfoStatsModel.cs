namespace HCDE.Gamedata;

/// <summary>EMAPINFO stats structs from <c>hcde_emapinfo.cpp</c> (33–50).</summary>
public sealed class VmEMapInfoUnsupportedSample
{
    public string MapName { get; set; } = "";
    public string Property { get; set; } = "";
    public int Line { get; set; }
}

public static class VmEMapInfoStatsModel
{
    public const int MaxUnsupportedSamples = 8;

    public static bool ShouldRecordSample(int currentCount) => currentCount < MaxUnsupportedSamples;
}
