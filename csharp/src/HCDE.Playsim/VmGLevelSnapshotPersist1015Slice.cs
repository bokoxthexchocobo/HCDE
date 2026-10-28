namespace HCDE.Playsim;

/// <summary>
/// Air friction, snapshot I/O, and visited persistence from <c>g_level.cpp</c> (**2109–2258**).
/// </summary>
public static class VmGLevelSnapshotPersist1015Slice
{
    public const double AirControlLowThreshold = 1.0 / 256.0;
    public const double AirFrictionLinearSlope = -0.0941;
    public const double AirFrictionLinearIntercept = 1.0004;
    public const uint LevelVisited = 0x80000000;
    public const int MaxPlayers = 8;

    public const string LevelSnapshotSuffix = ".map.json";
    public const string DefaultSnapshotSuffix = ".mapd.json";
    public const string VisitedArrayKey = "visited";
    public const string RandomClassesKey = "randomclasses";
    public const string PlayerClassesKey = "playerclasses";

    public enum SnapshotResourceKind
    {
        None,
        LevelMap,
        DefaultMap,
    }

    public readonly struct SnapshotWriteEntry
    {
        public string Filename { get; init; }
        public SnapshotResourceKind Kind { get; init; }
    }

    public static bool UsesLowAirFriction(double airControl) => airControl <= AirControlLowThreshold;

    public static double ComputeAirFriction(double airControl) =>
        UsesLowAirFriction(airControl)
            ? 1.0
            : airControl * AirFrictionLinearSlope + AirFrictionLinearIntercept;

    public static bool ShouldEmitSnapshot(long compressedSize) => compressedSize > 0;

    public static string BuildLevelSnapshotFilename(string mapName) =>
        $"{mapName}{LevelSnapshotSuffix}".ToLowerInvariant();

    public static string BuildDefaultSnapshotFilename(string mapName) =>
        $"{mapName}{DefaultSnapshotSuffix}".ToLowerInvariant();

    public static SnapshotWriteEntry PlanLevelSnapshotWrite(string mapName) =>
        new()
        {
            Filename = BuildLevelSnapshotFilename(mapName),
            Kind = SnapshotResourceKind.LevelMap,
        };

    public static SnapshotWriteEntry PlanDefaultSnapshotWrite(string mapName) =>
        new()
        {
            Filename = BuildDefaultSnapshotFilename(mapName),
            Kind = SnapshotResourceKind.DefaultMap,
        };

    public static bool ShouldPersistRandomPlayerClasses(bool multiplayer, bool deathmatch) =>
        !multiplayer || !deathmatch;

    public static bool IsLevelMarkedVisited(uint levelFlags) =>
        (levelFlags & LevelVisited) != 0;

    public static uint ApplyVisitedLevelFlag(uint levelFlags) =>
        levelFlags | LevelVisited;

    public static bool ShouldWriteVisitedMapName(uint levelFlags) =>
        IsLevelMarkedVisited(levelFlags);

    public static bool ShouldWritePlayerClassEntry(bool playerInGame, int playerIndex) =>
        playerInGame && playerIndex >= 0 && playerIndex < MaxPlayers;

    public static string FormatPlayerClassKey(int playerIndex) => $"{playerIndex}";

    public static SnapshotResourceKind ClassifySnapshotResourceName(string? entryName)
    {
        if (string.IsNullOrEmpty(entryName))
            return SnapshotResourceKind.None;
        if (entryName.EndsWith(LevelSnapshotSuffix, StringComparison.OrdinalIgnoreCase))
            return SnapshotResourceKind.LevelMap;
        if (entryName.EndsWith(DefaultSnapshotSuffix, StringComparison.OrdinalIgnoreCase))
            return SnapshotResourceKind.DefaultMap;
        return SnapshotResourceKind.None;
    }

    public static bool TryParseSnapshotMapName(string entryName, out string mapName, out SnapshotResourceKind kind)
    {
        mapName = string.Empty;
        kind = ClassifySnapshotResourceName(entryName);
        if (kind == SnapshotResourceKind.None)
            return false;

        var suffix = kind == SnapshotResourceKind.LevelMap ? LevelSnapshotSuffix : DefaultSnapshotSuffix;
        mapName = entryName[..^suffix.Length];
        return mapName.Length > 0;
    }

    public static bool ShouldAssignSnapshotToLevelInfo(SnapshotResourceKind kind) =>
        kind == SnapshotResourceKind.LevelMap;

    public static bool ShouldAssignSnapshotToDefaultLevelInfo(SnapshotResourceKind kind) =>
        kind == SnapshotResourceKind.DefaultMap;

    public static bool ShouldApplyVisitedFlagToLevelInfo(bool levelInfoFound, string? mapName) =>
        levelInfoFound && !string.IsNullOrEmpty(mapName);

    public static int CountVisitedEntriesToRead(int arraySize, int maxEntries) =>
        arraySize < 0 ? 0 : Math.Min(arraySize, maxEntries);
}
