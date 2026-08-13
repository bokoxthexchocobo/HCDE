namespace HCDE.Net.Core.Tests;

public class WorldDeltaApplySessionTests
{
    private sealed class RecordingWorldSink : IWorldDeltaApplySink
    {
        public List<byte> AppliedPlayers { get; } = new();

        public bool ApplyPose(int recipientClientSlot, PlayerPoseWorldDelta pose, int sequenceAck)
        {
            AppliedPlayers.Add(pose.PlayerNum);
            return true;
        }

        public bool ApplySector(SectorWorldDelta sector) => true;
    }

    [Fact]
    public void Apply_InvokesSinkForHasActorPoses()
    {
        var sink = new RecordingWorldSink();
        var header = new ServerWorldDeltaHeader(flags: 0, gameTic: 9, recordCount: 1);
        var poses = new[]
        {
            new PlayerPoseWorldDelta(
                playerNum: 0,
                flags: LiveConstants.ServerWorldDeltaPoseHasActor,
                health: 100,
                armor: 50,
                posX: 1,
                posY: 2,
                posZ: 3,
                velX: 0,
                velY: 0,
                velZ: 0,
                yawBams: 0,
                pitchBams: 0),
        };

        Assert.True(WorldDeltaApplySession.TryApply(
            header,
            poses,
            Array.Empty<SectorWorldDelta>(),
            snapshotPlayersMask: 1,
            recipientClientSlot: 1,
            sequenceAck: 42,
            sink,
            out var result,
            out _));

        Assert.Equal(1, result.PosesApplied);
        Assert.Equal(new byte[] { 0 }, sink.AppliedPlayers);
    }

    [Fact]
    public void Apply_RejectsDuplicatePlayerPose()
    {
        var header = new ServerWorldDeltaHeader(flags: 0, gameTic: 1, recordCount: 2);
        var poses = new[]
        {
            new PlayerPoseWorldDelta(0, LiveConstants.ServerWorldDeltaPoseHasActor, 100, 0, 0, 0, 0, 0, 0, 0, 0, 0),
            new PlayerPoseWorldDelta(0, LiveConstants.ServerWorldDeltaPoseHasActor, 90, 0, 0, 0, 0, 0, 0, 0, 0, 0),
        };

        Assert.False(WorldDeltaApplySession.TryApply(
            header,
            poses,
            Array.Empty<SectorWorldDelta>(),
            snapshotPlayersMask: 1,
            recipientClientSlot: 1,
            sequenceAck: 0,
            sink: null,
            out _,
            out var rejectReason));

        Assert.Equal("world-delta-duplicate-player", rejectReason);
    }
}

public class ActorDeltasApplySessionTests
{
    private sealed class RecordingActorSink : IActorDeltaApplySink
    {
        public List<uint> ActorIds { get; } = new();

        public bool TryApply(int recipientClientSlot, ActorDeltaRecord record)
        {
            ActorIds.Add(record.ActorId);
            return true;
        }
    }

    [Fact]
    public void Apply_RoutesRecordsToSink()
    {
        var sink = new RecordingActorSink();
        var record = new ActorDeltaRecord
        {
            ActorId = 77,
            ClassId = 3,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 50,
        };
        var header = new ActorDeltasHeader(LiveConstants.ActorDeltasFlagComplete, recordCount: 1);

        Assert.True(ActorDeltasApplySession.TryApply(
            header,
            new[] { record },
            recipientClientSlot: 1,
            sink,
            out var result,
            out _));

        Assert.Equal(1, result.Applied);
        Assert.Equal(0, result.Missing);
        Assert.Single(sink.ActorIds);
        Assert.Equal(77u, sink.ActorIds[0]);
    }
}

public class CoopDeadSpawnsApplySessionTests
{
    private sealed class RecordingDeadSpawnSink : ICoopDeadSpawnsApplySink
    {
        public List<uint> Indices { get; } = new();

        public bool TryRetireSpawnIndex(uint spawnIndex)
        {
            Indices.Add(spawnIndex);
            return true;
        }
    }

    [Fact]
    public void Apply_RetiresSpawnIndices()
    {
        var sink = new RecordingDeadSpawnSink();
        var header = new CoopDeadSpawnsHeader(flags: 0, recordCount: 2);

        Assert.True(CoopDeadSpawnsApplySession.TryApply(
            header,
            new uint[] { 10, 20 },
            sink,
            out var result,
            out _));

        Assert.Equal(2, result.Applied);
        Assert.Equal(new uint[] { 10u, 20u }, sink.Indices);
    }
}

public class ServerSnapshotTailParsedBlocksTests
{
    [Fact]
    public void CoopShippingTail_ExposesParsedWorldAndActorBlocks()
    {
        var pose = new PlayerPoseWorldDelta(
            playerNum: 0,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor,
            health: 100,
            armor: 0,
            posX: 64,
            posY: -32,
            posZ: 16,
            velX: 0,
            velY: 0,
            velZ: 0,
            yawBams: 0x40000000,
            pitchBams: 0);
        var actor = new ActorDeltaRecord
        {
            ActorId = 5,
            ClassId = 2,
            FieldMask = LiveConstants.ActorDeltaFieldHealth,
            Health = 80,
        };

        Span<byte> tail = stackalloc byte[512];
        var written = ServerSnapshotTailCodec.WriteCoopShipping(
            tail,
            gameTic: 12,
            poses: new[] { pose },
            sectors: ReadOnlySpan<SectorWorldDelta>.Empty,
            actorDeltas: new[] { actor },
            coopDeadSpawnIndices: new uint[] { 99 });

        Assert.True(ServerSnapshotTailWalker.TryWalk(tail[..written], out var sections, out _, out _));
        Assert.NotNull(sections.WorldDeltaPoses);
        Assert.Single(sections.WorldDeltaPoses!);
        Assert.NotNull(sections.ActorDeltaRecords);
        Assert.Single(sections.ActorDeltaRecords!);
        Assert.NotNull(sections.CoopDeadSpawnIndices);
        Assert.Equal(new uint[] { 99 }, sections.CoopDeadSpawnIndices);
    }
}
