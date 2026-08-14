namespace HCDE.Net.Core;

public readonly struct InvasionSpawnDirectory
{
    public InvasionSpawnDirectory(
        ushort totalSpotCount,
        ushort activeSpotCount,
        uint spawnPlanBudget,
        uint activeTag,
        bool usingFallback,
        byte fallbackSource,
        uint spawnedThisWave)
    {
        TotalSpotCount = totalSpotCount;
        ActiveSpotCount = activeSpotCount;
        SpawnPlanBudget = spawnPlanBudget;
        ActiveTag = activeTag;
        UsingFallback = usingFallback;
        FallbackSource = fallbackSource;
        SpawnedThisWave = spawnedThisWave;
    }

    public ushort TotalSpotCount { get; }
    public ushort ActiveSpotCount { get; }
    public uint SpawnPlanBudget { get; }
    public uint ActiveTag { get; }
    public bool UsingFallback { get; }
    public byte FallbackSource { get; }
    public uint SpawnedThisWave { get; }
}

public static class InvasionSpawnDirectoryCodec
{
    public static bool TryParseFromHeader(
        InvasionSnapshotHeader header,
        uint spawnedThisWave,
        out InvasionSpawnDirectory directory,
        out string? rejectReason)
    {
        directory = default;
        rejectReason = null;

        if (header.ProtocolVersion < 2)
            return true;

        if (header.ActiveSpawnSpotCount > header.SpawnSpotCount)
        {
            rejectReason = "invasion-spawn-active-count-overflow";
            return false;
        }

        directory = new InvasionSpawnDirectory(
            header.SpawnSpotCount,
            header.ActiveSpawnSpotCount,
            header.SpawnPlanBudget,
            header.SpawnActiveTag,
            (header.SpawnFlags & LiveConstants.InvasionSnapshotSpawnFlagUsingFallback) != 0,
            header.SpawnFallbackSource,
            spawnedThisWave);
        return true;
    }
}
