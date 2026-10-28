namespace HCDE.Gamedata;

/// <summary><c>G_ParseMapInfo</c> per-lump body and post-parse from <c>g_mapinfo.cpp</c> (3067–3114).</summary>
public static class VmMapInfoParseMapInfoLoop
{
    public const int LumpIndexUmapinfo = VmMapInfoLumpPrecedence.LumpIndexUmapinfo;
    public const int LumpIndexEmapinfo = VmMapInfoLumpPrecedence.LumpIndexEmapinfo;
    public const int LumpIndexZmapinfo = VmMapInfoLumpPrecedence.LumpIndexZmapinfo;
    public const uint Level3Secret = 1u;

    public enum LumpParseAction
    {
        ParseUmapinfoOnly,
        CommitThenParseEmapinfo,
        CommitThenParseNativeMapinfo
    }

    public static LumpParseAction ResolveParseAction(int nameIndex)
    {
        if (nameIndex == LumpIndexUmapinfo)
            return LumpParseAction.ParseUmapinfoOnly;
        if (nameIndex == LumpIndexEmapinfo)
            return LumpParseAction.CommitThenParseEmapinfo;
        return LumpParseAction.CommitThenParseNativeMapinfo;
    }

    public static bool UsesNewMapinfoFormat(int nameIndex) => nameIndex == LumpIndexZmapinfo;

    public static void ValidateEpisodesAndSkills(int episodeCount, int skillCount)
    {
        if (episodeCount == 0)
            throw new InvalidOperationException("clearepisodes without new episodes");
        if (skillCount == 0)
            throw new InvalidOperationException("clearskills without new skills");
    }

    public static bool ShouldCollectSecretMap(string nextSecret, string nextMap) =>
        !string.IsNullOrWhiteSpace(nextSecret) &&
        !string.Equals(nextSecret, nextMap, StringComparison.OrdinalIgnoreCase);

    public static uint ApplySecretLevelFlag(uint flags3) => flags3 | Level3Secret;
}
