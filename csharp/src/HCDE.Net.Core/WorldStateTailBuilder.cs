namespace HCDE.Net.Core;

public static class WorldStateTailBuilder
{
    public static int WriteCoopTailFromStore(
        Span<byte> tail,
        GuestWorldStateStore store,
        uint gameTic,
        uint[]? checksumHashes = null)
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
                armor: 0,
                posX: 0,
                posY: 0,
                posZ: 0,
                velX: 0,
                velY: 0,
                velZ: 0,
                yawBams: 0,
                pitchBams: 0);
        }

        var sectors = new SectorWorldDelta[store.Sectors.Count];
        var sectorIndex = 0;
        foreach (var sector in store.Sectors.Values.OrderBy(static s => s.SectorIndex))
        {
            sectors[sectorIndex++] = new SectorWorldDelta(
                sector.SectorIndex,
                flags: 0,
                sector.Floor,
                sector.Ceiling);
        }

        return ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic,
            poses,
            sectors,
            ReadOnlySpan<ActorDeltaRecord>.Empty,
            ReadOnlySpan<uint>.Empty,
            default,
            checksumHashes);
    }

    public static bool HasWorldDeltaPayload(GuestWorldStateStore store) =>
        store.Players.Count > 0 || store.Sectors.Count > 0;
}
