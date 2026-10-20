namespace HCDE.Gamedata;

/// <summary>DeHackEd lookup helpers from <c>d_dehacked.cpp</c> (118–348).</summary>
public static class VmDehackedLookupCore
{
    public static int? ResolveInfoNameIndex(int index, int infoNameCount, bool dsdHacked) =>
        index < 0 ? null : index < infoNameCount ? index : dsdHacked ? index : null;

    public static bool IsValidSoundIndex(int index, int soundMapCount) =>
        index >= 0 && index < soundMapCount;

    public static string DsdHackedActorName(int index) => $"~Dsdhacked~{index}";

    public static string DsdHackedSoundName(int index) => $"~dsdhacked/#{index}";

    public static string DsSoundLumpName(string shortName) => $"ds{shortName}";
}

public static class VmDehackedDefaultInfo
{
    public const int StartHealth = 100;
    public const int BfgCells = 40;
    public const double ExplosionAlpha = 2.0 / 3.0;
}
