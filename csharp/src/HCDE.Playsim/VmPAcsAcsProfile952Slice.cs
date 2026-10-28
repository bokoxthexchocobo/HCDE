namespace HCDE.Playsim;

/// <summary>
/// <c>DACSThinker::DumpScriptStatus</c> tail, <c>ACSProfileInfo</c>, profile arrangement/clear/sort helpers,
/// and <c>ShowProfileData</c> head from <c>p_acs.cpp</c> (**11100–11249**). Exclusive to chunk **952**.
/// </summary>
public static class VmPAcsAcsProfile952Slice
{
    public const uint InitialMinInstrPerRun = uint.MaxValue;
    public const int ModNameBufferChars = 13;
    public const int ScriptNameBufferChars = 21;

    public struct AcsProfileStats
    {
        public uint TotalInstr;
        public uint NumRuns;
        public uint MinInstrPerRun;
        public uint MaxInstrPerRun;
    }

    public readonly record struct ProfileCollectorRef(int ModuleIndex, int EntryIndex, AcsProfileStats Stats);

    public static bool DumpScriptStatusLoopContinues(object? script) => script is not null;

    public static string FormatDumpScriptStatusLine(
        string scriptPresentation,
        VmPAcsIsClientSideScript.ScriptState state) =>
        $"{scriptPresentation}: {VmPAcsStartScript951Slice.ScriptStateName(state)}";

    public static AcsProfileStats CreateResetProfileStats() => new()
    {
        TotalInstr = 0,
        NumRuns = 0,
        MinInstrPerRun = InitialMinInstrPerRun,
        MaxInstrPerRun = 0,
    };

    public static AcsProfileStats ApplyProfileRun(AcsProfileStats stats, uint numInstr)
    {
        stats.TotalInstr += numInstr;
        stats.NumRuns++;
        if (numInstr < stats.MinInstrPerRun)
            stats.MinInstrPerRun = numInstr;
        if (numInstr > stats.MaxInstrPerRun)
            stats.MaxInstrPerRun = numInstr;
        return stats;
    }

    public static bool ShouldCollectFunctionProfile(int importNum) => importNum == 0;

    public static void ResetProfileStats(ref AcsProfileStats stats) => stats = CreateResetProfileStats();

    public static int CompareProfileTotalInstr(in AcsProfileStats a, in AcsProfileStats b) =>
        (int)(b.TotalInstr - a.TotalInstr);

    public static int CompareProfileMinInstr(in AcsProfileStats a, in AcsProfileStats b) =>
        (int)(b.MinInstrPerRun - a.MinInstrPerRun);

    public static int CompareProfileMaxInstr(in AcsProfileStats a, in AcsProfileStats b) =>
        (int)(b.MaxInstrPerRun - a.MaxInstrPerRun);

    public static uint AverageInstrPerRun(in AcsProfileStats stats) =>
        stats.NumRuns == 0 ? 0 : stats.TotalInstr / stats.NumRuns;

    public static int CompareProfileAverageInstr(in AcsProfileStats a, in AcsProfileStats b) =>
        (int)AverageInstrPerRun(b) - (int)AverageInstrPerRun(a);

    public static int CompareProfileNumRuns(in AcsProfileStats a, in AcsProfileStats b) =>
        (int)(b.NumRuns - a.NumRuns);

    public static bool ShowProfileDataShouldReturn(int profileCount) => profileCount == 0;

    public static uint ResolveProfileDisplayLimit(int ilimit) =>
        ilimit > 0 ? (uint)ilimit : uint.MaxValue;

    public static string ProfileTypeLabel(bool functions) => functions ? "function" : "script";

    public static string ProfileListingTitle(bool functions, int ilimit) =>
        ilimit > 0
            ? $"Top {ilimit} {ProfileTypeLabel(functions)}s:"
            : $"All {ProfileTypeLabel(functions)}s:";

    public static bool ShouldSkipUnrunProfile(in AcsProfileStats stats) => stats.NumRuns == 0;

    public static bool ProfileRowWithinLimit(uint index, uint limit, int profileCount) =>
        index < limit && index < (uint)profileCount;
}
