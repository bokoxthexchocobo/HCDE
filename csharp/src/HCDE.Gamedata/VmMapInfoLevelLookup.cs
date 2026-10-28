namespace HCDE.Gamedata;

public sealed class VmMapInfoLevelRecord
{
    public string MapName { get; init; } = "";
    public int LevelNum { get; set; }
    public int WarpTrans { get; set; }
    public string LevelName { get; set; } = "";
    public string SkyPic1 { get; set; } = "SKY1";
    public string SkyPic2 { get; set; } = "SKY1";
    public string SkyMistPic { get; set; } = "SKYMIST1";
}

/// <summary><c>FindLevelInfo</c> / <c>FindWadLevelInfo</c> / <c>FindLevelByWarpTrans</c>.</summary>
public static class VmMapInfoLevelLookup
{
    public static int FindWadLevelIndex(IReadOnlyList<VmMapInfoLevelRecord> levels, string mapName)
    {
        for (var i = 0; i < levels.Count; i++)
        {
            if (levels[i].MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static VmMapInfoLevelRecord? GetLevelOrDefault(
        IReadOnlyList<VmMapInfoLevelRecord> levels,
        string mapName,
        bool allowDefault,
        VmMapInfoLevelRecord defaults)
    {
        var idx = FindWadLevelIndex(levels, mapName);
        if (idx >= 0)
            return levels[idx];
        if (!allowDefault)
            return null;
        if (string.IsNullOrEmpty(defaults.LevelName))
        {
            defaults.LevelName = "Unnamed";
            defaults.SkyPic1 = "SKY1";
            defaults.SkyPic2 = "SKY1";
            defaults.SkyMistPic = "SKYMIST1";
        }

        return defaults;
    }

    public static VmMapInfoLevelRecord? FindLevelByWarpTrans(IReadOnlyList<VmMapInfoLevelRecord> levels, int warpTrans)
    {
        for (var i = levels.Count - 1; i >= 0; i--)
        {
            if (levels[i].WarpTrans == warpTrans)
                return levels[i];
        }

        return null;
    }
}
