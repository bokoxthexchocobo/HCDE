namespace HCDE.MapLoader;

/// <summary><c>MapLoader::LoadSideDefs2</c> sidedef read policy from <c>maploader.cpp</c> (2169–2218).</summary>
public static class VmMapLoaderLoadSideDefs2
{
    public const int StrifeLaserRowOffset = 102;

    public static bool ShouldPatchStrifeLaserRow(int gameType, string midTexture, int gameStrife) =>
        (gameType & gameStrife) != 0 && midTexture.StartsWith("LASERB01", StringComparison.Ordinal);

    public static int ApplyStrifeLaserRowOffset(int rowOffset) => rowOffset + StrifeLaserRowOffset;

    public static bool IsSectorIndexOutOfRange(int sectorIndex, int sectorCount) =>
        (uint)sectorIndex >= (uint)sectorCount;

    public static int ResolveSectorIndexOrZero(int sectorIndex, int sectorCount) =>
        IsSectorIndexOutOfRange(sectorIndex, sectorCount) ? 0 : sectorIndex;

    public static bool ShouldReportBadSector(bool outOfRange) => outOfRange;

    public static bool ShouldPassHexenBehaviorToProcessTextures(bool mapHasBehavior) => !mapHasBehavior;
}
