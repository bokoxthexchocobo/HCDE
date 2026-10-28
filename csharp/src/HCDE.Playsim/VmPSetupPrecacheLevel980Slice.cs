namespace HCDE.Playsim;

/// <summary>
/// `PrecacheLevel` body, `ClearPortals`, and `ClearLevelData` head from `p_setup.cpp` (**151–300**).
/// </summary>
public static class VmPSetupPrecacheLevel980Slice
{
    public const int HitWall = 2;
    public const int HitSky = 4;
    public const int InitTransferSky = 255;
    public const int PortsfSkyflatonly = 1;
    public const int PortsSkyviewpoint = 1;

    public static bool ShouldSkipPrecacheLevel(bool demoPlayback, bool dedicatedServer) =>
        demoPlayback || dedicatedServer;

    public static bool ShouldTrackActorClassInHitlist(object? actorClass, bool iteratorReturnedActor) =>
        iteratorReturnedActor && actorClass is not null;

    public static bool ShouldTrackResolvedPrecacheClass(object? resolvedClass, bool lookupSucceeded) =>
        lookupSucceeded && resolvedClass is not null;

    public static bool SideQualifiesForSkyTransferHitflag(
        bool sideIsFirstOnLine,
        int lineSpecial,
        int staticInitSpecial,
        int transferSkyArg) =>
        sideIsFirstOnLine
        && lineSpecial == staticInitSpecial
        && transferSkyArg == InitTransferSky;

    public static int MergeSidePrecacheHitflags(int baseHitflag, bool skyTransferActive) =>
        skyTransferActive ? baseHitflag | HitSky : baseHitflag;

    public static bool ShouldPrecacheLevelSkyTexture(bool textureValid, int textureIndex) =>
        textureValid && textureIndex >= 0;

    public static bool ShouldAddWallPrecacheWhenTextureExists(bool textureExists, int bitmask) =>
        textureExists && bitmask != 0;

    public static bool UseSoftwareRendererPrecachePath(bool hardwareRenderer) => !hardwareRenderer;

    public static int MinimumSectorPortalSlotsForDefaults(int existingCount) =>
        existingCount < 2 ? 2 : existingCount;

    public static bool DefaultSkyPortalUsesSkyFlatOnly(int portalIndex, int portalCount) =>
        portalCount >= 2 && (portalIndex == 0 || portalIndex == 1);

    public static bool ShouldReleaseSubsectorPortalCoverage(Array? coverageArray) =>
        coverageArray is not null && coverageArray.Length > 0;

    public static bool ShouldDeletePortalGroupEntry(object? portalGroup, bool portalGroupsNonEmpty) =>
        portalGroupsNonEmpty && portalGroup is not null;

    public static (int TotalMonsters, int TotalItems, int TotalSecrets, int Killed, int FoundItems, int FoundSecrets)
        ResetLevelStatistics()
    {
        return (0, 0, 0, 0, 0, 0);
    }

    public static (double Sky1, double Sky2, double HwSky1, double HwSky2, double HwSkymist)
        ResetSkyScrollPositions()
    {
        return (0.0, 0.0, 0.0, 0.0, 0.0);
    }

    public static int SectorTexturePrecacheAddsForSectorPlanes(int planeCount) =>
        planeCount >= 2 ? 6 : 0;

    public static int SideTexturePrecacheAddsPerSide(int sidesWithMidTexture) =>
        sidesWithMidTexture > 0 ? 3 : 0;

    public static bool ClearLevelDataScrubsNativePointers(bool probeNotNull, int nativeFieldCount) =>
        probeNotNull && nativeFieldCount > 0;

    public static bool TryMergePrecacheClassEntry(
        Dictionary<object, bool> actorHitlist,
        object? actorClass,
        out bool added)
    {
        added = false;
        if (!ShouldTrackResolvedPrecacheClass(actorClass, lookupSucceeded: true))
            return false;

        if (actorHitlist.ContainsKey(actorClass!))
            return true;

        actorHitlist[actorClass!] = true;
        added = true;
        return true;
    }
}
