namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadSectors</c> policy from <c>maploader.cpp</c> (1056–1146).</summary>
public static class VmMapLoaderSectorInit
{
    public const int SectorOutsideSpecial = 1; // Sector_Outside low byte placeholder for tests

    public static int ResolveDefaultSeqType(bool levelSndSeqTotalCtrl) =>
        levelSndSeqTotalCtrl ? 0 : -1;

    public static bool ShouldSetFloorDropFlag(bool mapHasBehavior) => !mapHasBehavior;

    public static int TranslateSectorSpecial(bool mapHasBehavior, int rawSpecial, Func<int, int>? translator)
    {
        if (mapHasBehavior)
            return rawSpecial;
        return translator?.Invoke(rawSpecial) ?? rawSpecial;
    }

    public static uint ResolveFadeColor(
        uint outsideFog,
        bool ceilingIsSky,
        int specialLowByte,
        bool levelHasFadeTable,
        uint levelFadeTo)
    {
        if (outsideFog != 0xff000000u
            && (ceilingIsSky || (specialLowByte & 0xff) == SectorOutsideSpecial))
            return outsideFog;

        if (levelHasFadeTable)
            return 0x939393u;

        return levelFadeTo;
    }
}
