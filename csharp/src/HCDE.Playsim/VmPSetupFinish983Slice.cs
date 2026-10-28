namespace HCDE.Playsim;

/// <summary>
/// `P_SetupLevel` finish, `P_Init`, `P_Shutdown`, and debug CCMD head from `p_setup.cpp` (**601–750**).
/// </summary>
public static class VmPSetupFinish983Slice
{
    public const int FfExists = 1;
    public const int FfRenderplanes = 2;
    public const int SecmfIgnoreheightsec = 1 << 0;
    public const int LevelStartlightning = 1 << 0;
    public const int Level3Lightcreated = 1 << 0;
    public const int MapsectionScanLimit = 100;
    public const int SubsectorHackedFlag = 1;

    public static bool HeightsecIgnoreScanApplies(object? heightsec, int ffloorCount) =>
        heightsec is not null && ffloorCount > 0;

    public static bool Rendered3dFloorIgnoresHeightsec(int floorFlags) =>
        (floorFlags & (FfExists | FfRenderplanes)) == (FfExists | FfRenderplanes);

    public static int ApplyIgnoreHeightsecFlag(int moreFlags) =>
        moreFlags | SecmfIgnoreheightsec;

    public static int ScanSectorHeightsecIgnoreFlags(object? heightsec, ReadOnlySpan<int> ffloorFlags, int moreFlags)
    {
        if (!HeightsecIgnoreScanApplies(heightsec, ffloorFlags.Length))
            return moreFlags;

        var flags = moreFlags;
        foreach (var ff in ffloorFlags)
        {
            if (Rendered3dFloorIgnoresHeightsec(ff))
                flags = ApplyIgnoreHeightsecFlag(flags);
        }

        return flags;
    }

    public static bool ShouldSnapshotLoadMapGeometry(int elementCount) => elementCount > 0;

    public static bool ShouldEnableCompatLineOnSide(bool automapInitialized) => automapInitialized;

    public static bool ShouldStartLightningForLevel(int levelFlags) =>
        (levelFlags & LevelStartlightning) != 0;

    public static int MarkDynamicLightsCreated(int levelFlags3) =>
        levelFlags3 | Level3Lightcreated;

    public static bool ShouldInitializeDynamicLightsForActor(object? actor, bool lightsCreatedFlagSet) =>
        lightsCreatedFlagSet && actor is not null;

    public static bool ShutdownDestroysClientSideStatics(bool levelInShutdownLoop, bool clientSideListUsed) =>
        levelInShutdownLoop && clientSideListUsed;

    public static bool ShutdownClearsPlayerPsprites(object? psprites, bool playerAllocated) =>
        playerAllocated && psprites is not null;

    public static bool DumpGeometryLogsSector(int sectorIndex, bool commandActive) =>
        commandActive && sectorIndex >= 0;

    public static bool DumpGeometrySubsectorInRange(int subsectorIndex, int subsectorCount) =>
        subsectorIndex >= 0 && subsectorIndex < subsectorCount;

    public static bool DumpGeometrySubsectorHacked(int hackedField) =>
        (hackedField & SubsectorHackedFlag) != 0;

    public static bool DumpGeometrySegLoopContinues(int segIndex, int segCount) =>
        segIndex >= 0 && segIndex < segCount;

    public static bool DumpGeometrySegHasLinedef(object? linedef, bool segValid) =>
        segValid && linedef is not null;

    public static bool DumpGeometrySegIsMiniseg(object? linedef, bool segValid) =>
        segValid && linedef is null;

    public static bool DumpGeometryLogsBackSectorViaPartner(object? partnerSeg, object? backsector) =>
        partnerSeg is not null || backsector is not null;

    public static bool MapsectionScanIndexValid(int sectionIndex) =>
        sectionIndex >= 0 && sectionIndex < MapsectionScanLimit;

    public static bool ListMapsectionsScansSubsectors(int sectionIndex, int subsectorCount, int mapsectionId) =>
        MapsectionScanIndexValid(sectionIndex) && subsectorCount > 0 && mapsectionId == sectionIndex;
}
