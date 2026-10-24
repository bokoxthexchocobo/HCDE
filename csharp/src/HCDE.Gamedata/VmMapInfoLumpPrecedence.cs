namespace HCDE.Gamedata;

/// <summary><c>G_ParseMapInfo</c> multi-lump scan precedence from <c>g_mapinfo.cpp</c>.</summary>
public static class VmMapInfoLumpPrecedence
{
    public const int LumpIndexMapinfo = 0;
    public const int LumpIndexZmapinfo = 1;
    public const int LumpIndexUmapinfo = 2;
    public const int LumpIndexEmapinfo = 3;

    /// <summary>MAPINFO lump is skipped when ZMAPINFO exists in the same archive.</summary>
    public static bool ShouldSkipClassicMapinfo(bool sameWadHasZmapinfo) => sameWadHasZmapinfo;

    /// <summary>UMAPINFO is skipped when ZMAPINFO or MAPINFO exists in the same WAD.</summary>
    public static bool ShouldSkipUmapinfo(bool sameWadHasZmapinfo, bool sameWadHasMapinfo) =>
        sameWadHasZmapinfo || sameWadHasMapinfo;

    /// <summary>EMAPINFO is skipped when any native map-info lump exists in the same archive.</summary>
    public static bool ShouldSkipEmapinfo(bool sameWadHasZmapinfo, bool sameWadHasMapinfo, bool sameWadHasUmapinfo) =>
        sameWadHasZmapinfo || sameWadHasMapinfo || sameWadHasUmapinfo;

    public static bool ShouldSkipLump(int nameIndex, bool hasZmapinfo, bool hasMapinfo, bool hasUmapinfo)
    {
        return nameIndex switch
        {
            LumpIndexMapinfo => ShouldSkipClassicMapinfo(hasZmapinfo),
            LumpIndexUmapinfo => ShouldSkipUmapinfo(hasZmapinfo, hasMapinfo),
            LumpIndexEmapinfo => ShouldSkipEmapinfo(hasZmapinfo, hasMapinfo, hasUmapinfo),
            _ => false
        };
    }
}
