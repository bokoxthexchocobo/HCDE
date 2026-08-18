using System.Buffers.Binary;

namespace HCDE.Net.Core;

public static class SnapshotChecksumPlaysimInputs
{
    public static SnapshotChecksumInputs Build(GuestWorldStateStore store, int gameTic, int rngSeed)
    {
        var players = new SnapshotChecksumPlayerSample[store.Players.Count];
        var playerIndex = 0;
        foreach (var player in store.Players.Values.OrderBy(static p => p.PlayerNum))
        {
            players[playerIndex++] = new SnapshotChecksumPlayerSample(
                player.PlayerNum,
                player.Health,
                player.PlayerState,
                player.OnGround);
        }

        var sectors = new SnapshotChecksumSectorSample[store.Sectors.Count];
        var sectorIndex = 0;
        foreach (var sector in store.Sectors.Values.OrderBy(static s => s.SectorIndex))
        {
            sectors[sectorIndex++] = new SnapshotChecksumSectorSample(
                sector.SectorIndex,
                sector.Floor,
                sector.Ceiling,
                sector.LightLevel,
                sector.Special);
        }

        var movers = Array.Empty<SnapshotChecksumMoverSample>();

        var actors = new SnapshotChecksumActorSample[store.Actors.Count];
        var actorIndex = 0;
        foreach (var actor in store.Actors.Values.OrderBy(static a => a.ActorId))
        {
            Span<byte> className = stackalloc byte[2];
            BinaryPrimitives.WriteUInt16BigEndian(className, actor.ClassId);
            actors[actorIndex++] = new SnapshotChecksumActorSample(className, actor.Health);
        }

        return new SnapshotChecksumInputs(
            players,
            sectors,
            movers,
            actors,
            rngSeed,
            gameTic,
            lineSpecRollingHash: SnapshotChecksumLineSpecPolicy.ComputeRollingHash(store),
            coopDeadSpawnRollingHash: SnapshotChecksumCoopDeadSpawnPolicy.ComputeRollingHash(store),
            authorityEventRollingHash: SnapshotChecksumAuthorityEventPolicy.ComputeRollingHash(store),
            actorDeltaRollingHash: SnapshotChecksumActorDeltaPolicy.ComputeRollingHash(store),
            presentationEchoRollingHash: SnapshotChecksumPresentationEchoPolicy.ComputeRollingHash(store));
    }

    public static void ComputeAndStore(
        SnapshotChecksumSession session,
        GuestWorldStateStore store,
        int gameTic,
        int rngSeed,
        byte categoryMask = SnapshotChecksumRing.DefaultEnabledCategoryMask)
    {
        var inputs = Build(store, gameTic, rngSeed);
        session.ComputeIfStale(gameTic, inputs, categoryMask);
    }
}
