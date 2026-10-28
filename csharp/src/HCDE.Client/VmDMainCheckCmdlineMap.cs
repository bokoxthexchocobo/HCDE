namespace HCDE.Client;

/// <summary><c>CheckCmdLine</c> map, skill, and status tail from <c>d_main.cpp</c> (2682–2800).</summary>
public static class VmDMainCheckCmdlineMap
{
    public static string DefaultStartMap(bool isHexen, bool mapxx) =>
        isHexen ? "&wt@01" : mapxx ? "MAP01" : "E1M1";

    public static bool TryParseSkillDigit(string? val, out int skillIndex)
    {
        skillIndex = -1;
        if (string.IsNullOrEmpty(val))
            return false;
        skillIndex = val[0] - '1';
        return true;
    }

    public static (int episode, int map) ParseWarpMapxx(int warpArg) => (1, warpArg);

    public static (int episode, int map) ParseWarpEpisode(string epArg, string? mapArg)
    {
        var ep = int.Parse(epArg);
        var map = mapArg is null ? 10 : int.Parse(mapArg);
        if (map < 1 || map > 9)
            return (1, ep);
        return (ep, map);
    }

    public static string FormatCalcMapName(int episode, int map) => $"E{episode}M{map}";

    public static bool IsLauncherDefaultMap(string map) =>
        map.Equals("MAP01", StringComparison.OrdinalIgnoreCase)
        || map.Equals("E1M1", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldSetMapFromPlusMap(bool mapExists) => mapExists;

    public static bool ShouldMarkLauncherDefault(bool launcherDefault, string? compatOverride) =>
        launcherDefault && compatOverride is null;

    public static string FormatTimelimitMessage(double minutes) =>
        $"Levels will end after {minutes} minute{(minutes > 1 ? "s" : "")}.\n";

    public static bool ShouldAppendDeathmatchStatus(bool deathmatch) => deathmatch;

    public static bool ShouldAppendNoMonstersStatus(int dmflags, int dfNoMonsters) =>
        (dmflags & dfNoMonsters) != 0;

    public static string FormatWarpStatusLine(string startMap, int skillOneBased) =>
        $"Warp to map {startMap}, Skill {skillOneBased} ";
}
