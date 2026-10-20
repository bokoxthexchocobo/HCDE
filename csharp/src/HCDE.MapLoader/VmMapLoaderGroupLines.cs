namespace HCDE.MapLoader;

/// <summary>Opening of <c>MapLoader::GroupLines</c> from <c>maploader.cpp</c> (2659–2705).</summary>
public static class VmMapLoaderGroupLines
{
    public static bool ShouldFlagMissingFrontSector(bool frontSectorNull, bool alreadyFlagged) =>
        frontSectorNull && !alreadyFlagged;

    public static bool ShouldCountBackSectorLine(bool hasBackSector, bool sameAsFront) =>
        hasBackSector && !sameAsFront;

    public static int IncrementLineCount(int total) => total + 1;
}
