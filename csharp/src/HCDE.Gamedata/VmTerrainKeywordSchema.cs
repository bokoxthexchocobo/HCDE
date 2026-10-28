namespace HCDE.Gamedata;

/// <summary>Keyword tables and parser metadata from <c>p_terrain.cpp</c>.</summary>
public static class VmTerrainKeywordSchema
{
    public static IReadOnlyList<string> OuterKeywords =>
    [
        "splash", "terrain", "floor", "ifdoom", "ifheretic", "ifhexen", "ifstrife", "endif", "defaultterrain"
    ];

    public static IReadOnlyList<string> SplashKeywords =>
    [
        "}", "smallsound", "smallclip", "sound", "smallclass", "baseclass", "chunkclass",
        "chunkxvelshift", "chunkyvelshift", "chunkzvelshift", "chunkbasezvel", "noalert"
    ];

    public static IReadOnlyList<string> TerrainKeywords =>
    [
        "}", "splash", "damageamount", "damagetype", "damagetimemask", "footclip", "stepvolume",
        "walksteptics", "runsteptics", "leftstepsounds", "rightstepsounds", "liquid", "friction",
        "allowprotection", "damageonland", "stepsounds", "stepdistance", "stepdistanceminvel"
    ];

    public static int SplashParserFieldCount => 11;

    public static int TerrainParserFieldCount => 18;
}
