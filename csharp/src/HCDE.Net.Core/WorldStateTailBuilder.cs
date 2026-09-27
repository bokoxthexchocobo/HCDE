namespace HCDE.Net.Core;

public readonly struct WorldStateTailBuildResult
{
    public WorldStateTailBuildResult(bool hasTail, int bytesWritten)
    {
        HasTail = hasTail;
        BytesWritten = bytesWritten;
    }

    public bool HasTail { get; }
    public int BytesWritten { get; }
}

public static class WorldStateTailBuilder
{
    public static int WriteCoopTailFromStore(
        Span<byte> tail,
        GuestWorldStateStore store,
        uint gameTic,
        uint[]? checksumHashes = null,
        bool replicateSectorMetadata = false)
    {
        var poses = CollectPoses(store);
        var sectors = CollectSectors(store, replicateSectorMetadata);
        var actors = CollectActorDeltas(store);
        var retired = store.PeekPendingCoopDeadSpawnsForTail();
        var events = store.PeekPendingAuthorityEventsForTail();
        var written = ServerSnapshotTailCodec.WriteCoopShipping(
            tail, gameTic, poses, sectors, actors, retired, events, checksumHashes);
        if (written > 0)
            CommitShippedRecords(store, actors);
        return written;
    }

    public static bool HasWorldDeltaPayload(GuestWorldStateStore store) =>
        store.Players.Count > 0 || store.Sectors.Count > 0 || store.Actors.Count > 0
            || store.HasPendingCoopDeadSpawns || store.HasPendingAuthorityEvents;

    public static WorldStateTailBuildResult TryBuildCoopTailFromStore(
        Span<byte> tail,
        GuestWorldStateStore store,
        uint gameTic,
        uint[]? checksumHashes = null,
        bool replicateSectorMetadata = false)
    {
        if (!HasWorldDeltaPayload(store))
            return default;

        var written = WriteCoopTailFromStore(
            tail,
            store,
            gameTic,
            checksumHashes,
            replicateSectorMetadata);
        return new WorldStateTailBuildResult(written > 0, written);
    }

    public static WorldStateTailBuildResult TryBuildCoopTailWithChecksum(
        Span<byte> tail, GuestWorldStateStore store, SnapshotChecksumSession? checksumSession,
        uint gameTic, int rngSeed = 0, bool replicateSectorMetadata = false)
    {
        var poses = CollectPoses(store);
        var sectors = CollectSectors(store, replicateSectorMetadata);
        var events = store.PeekPendingAuthorityEventsForTail();
        var actors = CollectActorDeltas(store);
        var retired = store.PeekPendingCoopDeadSpawnsForTail();
        // Validate the entire tail before consuming events or advancing rolling hashes.
        // The placeholder reserves exactly the checksum block that the final write uses.
        var placeholder = checksumSession is null ? null : new uint[LiveConstants.SnapshotChecksumCategoryCount];
        if (ServerSnapshotTailCodec.WriteCoopShipping(tail, gameTic, poses, sectors, actors, retired, events, placeholder) == 0)
            return default;
        CommitShippedRecords(store, actors);
        var hashes = SnapshotChecksumTailPolicy.TryResolveTailChecksumHashes(store, checksumSession, (int)gameTic, rngSeed);
        var written = ServerSnapshotTailCodec.WriteCoopShipping(tail, gameTic, poses, sectors, actors, retired, events, hashes);
        return new WorldStateTailBuildResult(written > 0, written);
    }

    public static int WriteInvasionTail(
        Span<byte> tail,
        uint gameTic,
        InvasionSnapshotHeader invasionSnapshot,
        uint[]? checksumHashes = null)
        => ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic,
            invasionSnapshot,
            checksumHashes: checksumHashes);

    public static WorldStateTailBuildResult TryBuildInvasionTail(
        Span<byte> tail,
        uint gameTic,
        InvasionSnapshotHeader invasionSnapshot,
        uint[]? checksumHashes = null)
    {
        var written = WriteInvasionTail(tail, gameTic, invasionSnapshot, checksumHashes);
        return new WorldStateTailBuildResult(written > 0, written);
    }

    public static uint[]? TryComputeChecksumHashes(
        GuestWorldStateStore? store,
        SnapshotChecksumSession? checksumSession,
        int gameTic,
        int rngSeed = 0)
    {
        if (store is null || checksumSession is null)
            return null;

        SnapshotChecksumPlaysimInputs.ComputeAndStore(checksumSession, store, gameTic, rngSeed);
        return checksumSession.Ring.TryFind(gameTic, out var hashes) ? hashes : null;
    }

    public static WorldStateTailBuildResult TryBuildInvasionTailWithChecksum(
        Span<byte> tail,
        GuestWorldStateStore? store,
        SnapshotChecksumSession? checksumSession,
        uint gameTic,
        InvasionSnapshotHeader invasionSnapshot,
        int rngSeed = 0)
    {
        var placeholder = store is null || checksumSession is null ? null : new uint[LiveConstants.SnapshotChecksumCategoryCount];
        if (WriteInvasionTail(tail, gameTic, invasionSnapshot, placeholder) == 0)
            return default;
        var checksumHashes = SnapshotChecksumTailPolicy.TryResolveTailChecksumHashes(
            store,
            checksumSession,
            (int)gameTic,
            rngSeed);
        return TryBuildInvasionTail(tail, gameTic, invasionSnapshot, checksumHashes);
    }

    public static WorldStateTailBuildResult TryBuildMergedInvasionCoopTail(
        Span<byte> tail,
        GuestWorldStateStore store,
        SnapshotChecksumSession? checksumSession,
        uint gameTic,
        InvasionSnapshotHeader invasionSnapshot,
        int rngSeed = 0,
        bool replicateSectorMetadata = false)
    {
        var poses = CollectPoses(store);
        var sectors = CollectSectors(store, replicateSectorMetadata);
        var embeddedAuthorityEvents = store.PeekPendingAuthorityEventsForTail();
        var embeddedActorDeltas = CollectActorDeltas(store);
        var embeddedCoopDeadSpawns = store.PeekPendingCoopDeadSpawnsForTail();
        var placeholder = checksumSession is null ? null : new uint[LiveConstants.SnapshotChecksumCategoryCount];
        if (ServerSnapshotTailCodec.WriteInvasionShipping(tail, gameTic, invasionSnapshot,
            poses, sectors, embeddedAuthorityEvents, embeddedActorDeltas, embeddedCoopDeadSpawns, placeholder) == 0)
            return default;
        CommitShippedRecords(store, embeddedActorDeltas);
        var checksumHashes = SnapshotChecksumTailPolicy.TryResolveTailChecksumHashes(
            store,
            checksumSession,
            (int)gameTic,
            rngSeed);
        var written = ServerSnapshotTailCodec.WriteInvasionShipping(
            tail,
            gameTic,
            invasionSnapshot,
            poses,
            sectors,
            embeddedAuthorityEvents,
            embeddedActorDeltas,
            embeddedCoopDeadSpawns,
            checksumHashes);
        return new WorldStateTailBuildResult(written > 0, written);
    }

    private static void CommitShippedRecords(GuestWorldStateStore store, ActorDeltaRecord[] actors)
    {
        store.TakePendingAuthorityEventsForTail();
        store.MixShippedActorDeltas(actors);
        store.TakePendingCoopDeadSpawnsForTail();
    }

    private static PlayerPoseWorldDelta[] CollectPoses(GuestWorldStateStore store)
    {
        var poses = new PlayerPoseWorldDelta[store.Players.Count];
        var poseIndex = 0;
        foreach (var player in store.Players.Values.OrderBy(static p => p.PlayerNum))
        {
            var flags = LiveConstants.ServerWorldDeltaPoseHasActor;
            if (player.OnGround)
                flags |= LiveConstants.ServerWorldDeltaPoseOnGround;

            poses[poseIndex++] = new PlayerPoseWorldDelta(
                player.PlayerNum,
                flags,
                player.Health,
                player.Armor,
                player.PosX,
                player.PosY,
                player.PosZ,
                player.VelX,
                player.VelY,
                player.VelZ,
                player.YawBams,
                player.PitchBams);
        }

        return poses;
    }

    private static SectorWorldDelta[] CollectSectors(
        GuestWorldStateStore store,
        bool replicateSectorMetadata)
    {
        var sectors = new SectorWorldDelta[store.Sectors.Count];
        var sectorIndex = 0;
        foreach (var sector in store.Sectors.Values.OrderBy(static s => s.SectorIndex))
        {
            byte flags = 0;
            if (replicateSectorMetadata)
            {
                flags |= LiveConstants.ServerWorldDeltaSectorHasLight;
                flags |= LiveConstants.ServerWorldDeltaSectorHasSpecial;
            }

            sectors[sectorIndex++] = new SectorWorldDelta(
                sector.SectorIndex,
                flags,
                sector.Floor,
                sector.Ceiling,
                sector.LightLevel,
                sector.Special);
        }

        return sectors;
    }

    private static ActorDeltaRecord[] CollectActorDeltas(GuestWorldStateStore store)
    {
        var actorDeltas = new ActorDeltaRecord[store.Actors.Count];
        var actorIndex = 0;
        foreach (var actor in store.Actors.Values.OrderBy(static a => a.ActorId))
        {
            actorDeltas[actorIndex++] = new ActorDeltaRecord
            {
                ActorId = actor.ActorId,
                ClassId = actor.ClassId,
                FieldMask = (ushort)(LiveConstants.ActorDeltaFieldCategory
                    | LiveConstants.ActorDeltaFieldFlags
                    | LiveConstants.ActorDeltaFieldHealth
                    | (actor.HasPose ? LiveConstants.ActorDeltaFieldPos | LiveConstants.ActorDeltaFieldAngles : 0)),
                Category = actor.Category,
                Flags = actor.Flags,
                Health = actor.Health,
                PosX = actor.PosX,
                PosY = actor.PosY,
                PosZ = actor.PosZ,
                YawBams = actor.YawBams,
            };
        }
        return actorDeltas;
    }
}
