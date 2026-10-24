namespace HCDE.Gamedata;

/// <summary>MAPINFO document header constants from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoDocumentHeader
{
    public static IReadOnlyList<string> MapBlockKeys =>
        ["map", "cluster", "skill", "gameinfo", "include", "clearskills"];
}
