namespace HCDE.Gamedata;

/// <summary>
/// Level lookup helpers and <c>CheckWarpTransMap</c> head from <c>g_mapinfo.cpp</c> (**1–150**).
/// Chunk **1051**; uncredited <see cref="VmMapInfoLevelLookup"/> / <see cref="VmMapInfoWarpTrans"/> — seam only, not re-credited.
/// </summary>
public static class VmMapInfoParseHead1051Slice
{
    public const int ParsedLumpsInitialCapacity = 8;

    public static int FindWadLevelIndex(IReadOnlyList<VmMapInfoLevelRecord> levels, string mapName)
    {
        for (var i = 0; i < levels.Count; i++)
        {
            if (levels[i].MapName.Equals(mapName, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    public static VmMapInfoLevelRecord? FindLevelInfo(
        IReadOnlyList<VmMapInfoLevelRecord> levels,
        string mapName,
        bool allowDefault,
        VmMapInfoLevelRecord defaultLevel)
    {
        var idx = FindWadLevelIndex(levels, mapName);
        if (idx >= 0)
            return levels[idx];
        if (!allowDefault)
            return null;
        if (string.IsNullOrEmpty(defaultLevel.LevelName))
        {
            defaultLevel.SkyPic2 = defaultLevel.SkyPic1 = "SKY1";
            defaultLevel.SkyMistPic = "SKYMIST1";
            defaultLevel.LevelName = "Unnamed";
        }

        return defaultLevel;
    }

    public static VmMapInfoLevelRecord? FindLevelByNum(IReadOnlyList<VmMapInfoLevelRecord> levels, int levelNum)
    {
        foreach (var lev in levels)
        {
            if (lev.LevelNum == levelNum)
                return lev;
        }

        return null;
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

    public static bool IsWarpTransMapPrefix(string mapName) =>
        mapName.Length > 4
        && mapName[0] == '&'
        && (mapName[1] & 0xDF) == 'W'
        && (mapName[2] & 0xDF) == 'T'
        && mapName[3] == '@';

    /// <summary>Native <c>CheckWarpTransMap</c> through substitute <c>MAP</c> prefix build (**134–150** hull seam).</summary>
    public static bool TryResolveCheckWarpTransMap(
        string mapName,
        bool substitute,
        IReadOnlyList<VmMapInfoLevelRecord> levels,
        out string resolved)
    {
        resolved = mapName;
        if (!IsWarpTransMapPrefix(mapName))
            return false;

        if (int.TryParse(mapName.AsSpan(4), out var trans))
        {
            var lev = FindLevelByWarpTrans(levels, trans);
            if (lev != null)
            {
                resolved = lev.MapName;
                return true;
            }
        }

        if (substitute && mapName.Length > 5)
        {
            resolved = $"MAP{mapName[4]}{mapName[5]}";
            return false;
        }

        return false;
    }
}
