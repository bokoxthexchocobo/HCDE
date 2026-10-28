namespace HCDE.Gamedata;

/// <summary>
/// `SetLevelNum` and `FMapInfoParser::ParseMapInfo` head from <c>g_mapinfo.cpp</c> (**2600–2749**).
/// Chunk **1029**; primary **1491–1640** aborted (`VmMapInfoMapOptionsLate` **1421–1560**, `VmMapInfoScalarOptionsBlock` **1567–1716**, `VmMapInfoMapOptionsExitEnvironment` **1567–1652**).
/// </summary>
public static class VmMapInfoParseMapInfo1029Slice
{
    public const int FmtOld = 0;
    public const int FmtNew = 1;

    public sealed class LevelNumEntry
    {
        public int LevelNum { get; set; }
        public int Id24LevelNum { get; set; }
    }

    public static void ApplySetLevelNum(IList<LevelNumEntry> levels, LevelNumEntry target, int num)
    {
        foreach (var lev in levels)
        {
            if (lev == target)
                continue;
            if (lev.LevelNum == num)
                lev.LevelNum = 0;
        }

        target.LevelNum = num;
        target.Id24LevelNum = 0;
    }

    public static bool LumpAlreadyParsed(IReadOnlyCollection<int> parsedLumps, int lump) =>
        parsedLumps.Contains(lump);

    public static bool ShouldFatalOnCoreIncludeOverride(int parserFileContainer, int includeFileContainer) =>
        parserFileContainer == 0 && includeFileContainer != parserFileContainer;

    public static string ResolveSkyPic2WhenNoFlat(string skyPic2, string skyPic1) =>
        skyPic2.Equals("-NOFLAT-", StringComparison.OrdinalIgnoreCase) ? skyPic1 : skyPic2;

    public static bool TryParseTopLevelKeyword(
        string keyword,
        int formatType,
        out MapInfoTopLevelCommand command,
        out string? oldFormatError)
    {
        command = MapInfoTopLevelCommand.Unknown;
        oldFormatError = null;
        if (keyword.Equals("include", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.Include;
            return true;
        }

        if (keyword.Equals("gamedefaults", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.GameDefaults;
            return true;
        }

        if (keyword.Equals("defaultmap", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.DefaultMap;
            return true;
        }

        if (keyword.Equals("adddefaultmap", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.AddDefaultMap;
            return true;
        }

        if (keyword.Equals("map", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.Map;
            return true;
        }

        if (keyword.Equals("clusterdef", StringComparison.OrdinalIgnoreCase)
            || (formatType != FmtOld && keyword.Equals("cluster", StringComparison.OrdinalIgnoreCase)))
        {
            command = MapInfoTopLevelCommand.Cluster;
            return true;
        }

        if (keyword.Equals("episode", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.Episode;
            return true;
        }

        if (keyword.Equals("clearepisodes", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.ClearEpisodes;
            return true;
        }

        if (keyword.Equals("skill", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.Skill;
            return true;
        }

        if (keyword.Equals("clearskills", StringComparison.OrdinalIgnoreCase))
        {
            command = MapInfoTopLevelCommand.ClearSkills;
            return true;
        }

        if (keyword.Equals("gameinfo", StringComparison.OrdinalIgnoreCase))
        {
            if (formatType == FmtOld)
            {
                oldFormatError = "gameinfo definitions not supported with old MAPINFO syntax";
                return false;
            }

            command = MapInfoTopLevelCommand.GameInfo;
            return true;
        }

        if (keyword.Equals("intermission", StringComparison.OrdinalIgnoreCase))
        {
            if (formatType == FmtOld)
            {
                oldFormatError = "intermission definitions not supported with old MAPINFO syntax";
                return false;
            }

            command = MapInfoTopLevelCommand.Intermission;
            return true;
        }

        if (keyword.Equals("doomednums", StringComparison.OrdinalIgnoreCase))
        {
            if (formatType == FmtOld)
            {
                oldFormatError = "doomednums definitions not supported with old MAPINFO syntax";
                return false;
            }

            command = MapInfoTopLevelCommand.DoomedNums;
            return true;
        }

        return false;
    }

    public enum MapInfoTopLevelCommand
    {
        Unknown,
        Include,
        GameDefaults,
        DefaultMap,
        AddDefaultMap,
        Map,
        Cluster,
        Episode,
        ClearEpisodes,
        Skill,
        ClearSkills,
        GameInfo,
        Intermission,
        DoomedNums
    }
}
