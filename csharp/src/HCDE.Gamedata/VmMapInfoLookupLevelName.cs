namespace HCDE.Gamedata;

/// <summary><c>level_info_t::LookupLevelName</c> from <c>g_mapinfo.cpp</c> (342–389).</summary>
public static class VmMapInfoLookupLevelName
{
    public static string? StripLocalizedHeader(string mapName, string lookedUp)
    {
        var prefix = BuildHeaderPrefix(mapName);
        if (prefix.Length == 0)
            return lookedUp;

        var idx = lookedUp.IndexOf(prefix, StringComparison.Ordinal);
        return idx < 0 ? lookedUp : lookedUp[(idx + prefix.Length)..];
    }

    public static string BuildHeaderPrefix(string mapName)
    {
        if (mapName.Length > 3 && mapName[0] == 'E' && mapName[2] == 'M')
            return $"{mapName}: ";
        if (mapName.Length > 3 && mapName.StartsWith("MAP", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(mapName.AsSpan(3), out var mapNum))
            return $"{mapNum}: ";
        if (mapName.Length > 5 && mapName.StartsWith("LEVEL", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(mapName.AsSpan(5), out var levelNum))
            return $"{levelNum}: ";
        return "";
    }
}
