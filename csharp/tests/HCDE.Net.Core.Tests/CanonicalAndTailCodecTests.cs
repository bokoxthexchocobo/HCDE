namespace HCDE.Net.Core.Tests;

public class CanonicalEventPayloadCodecTests
{
    [Fact]
    public void Say_LegacyNullTerminatedString_BecomesLengthPrefixed()
    {
        var legacy = new byte[] { 0, (byte)'h', (byte)'i', 0 };
        Span<byte> output = stackalloc byte[16];
        var legacyCursor = 0;

        Assert.True(CanonicalEventPayloadCodec.TryBuildFromLegacy((byte)DemoCommand.Say, legacy, ref legacyCursor, output, out var length));
        Assert.Equal(5, length);
        Assert.Equal(legacy.Length, legacyCursor);
        Assert.Equal(new byte[] { 0, 0, 2, (byte)'h', (byte)'i' }, output[..length].ToArray());
    }

    [Fact]
    public void GenericCheat_CopiesSingleByte()
    {
        var legacy = new byte[] { 42 };
        Span<byte> output = stackalloc byte[8];
        var legacyCursor = 0;

        Assert.True(CanonicalEventPayloadCodec.TryBuildFromLegacy((byte)DemoCommand.GenericCheat, legacy, ref legacyCursor, output, out var length));
        Assert.Equal(1, length);
        Assert.Equal(42, output[0]);
    }

    [Fact]
    public void Readied_HasEmptyPayload()
    {
        Span<byte> output = stackalloc byte[8];
        var legacyCursor = 0;

        Assert.True(CanonicalEventPayloadCodec.TryBuildFromLegacy((byte)DemoCommand.Readied, ReadOnlySpan<byte>.Empty, ref legacyCursor, output, out var length));
        Assert.Equal(0, length);
    }
}

public class DemEventStreamConverterTests
{
    [Fact]
    public void MixedLegacyStream_ProducesCanonicalEventBlock()
    {
        var legacy = new byte[]
        {
            (byte)DemoCommand.Readied,
            (byte)DemoCommand.GenericCheat, 7,
            (byte)DemoCommand.Say, 0, (byte)'o', (byte)'k', 0,
        };

        Span<byte> output = stackalloc byte[64];
        Assert.True(DemEventStreamConverter.TryConvertToCanonical(legacy, clientInput: false, output, out var length));

        var cursor = 0;
        Assert.True(EventRecordsCodec.TryRead(output[..length], ref cursor, out var count, out _));
        Assert.Equal(3, count);
        Assert.Equal(length, cursor);
    }

    [Fact]
    public void ClientInputMode_DropsDisallowedEvents()
    {
        var legacy = new byte[]
        {
            (byte)DemoCommand.Print, (byte)'x', 0,
            (byte)DemoCommand.Readied,
        };

        Span<byte> output = stackalloc byte[64];
        Assert.True(DemEventStreamConverter.TryConvertToCanonical(legacy, clientInput: true, output, out var length));

        var cursor = 0;
        Assert.True(EventRecordsCodec.TryRead(output[..length], ref cursor, out var count, out _));
        Assert.Equal(1, count);
        Assert.Equal(length, cursor);
    }
}

public class WorldDeltaChunkCodecTests
{
    [Fact]
    public void EmptyChunk_RoundTrip()
    {
        Span<byte> chunk = stackalloc byte[32];
        Assert.Equal(LiveConstants.ServerWorldDeltaHeaderSize + 1, WorldDeltaChunkCodec.WriteEmpty(chunk, gameTic: 99));

        Assert.True(WorldDeltaChunkCodec.TryRead(chunk[..(LiveConstants.ServerWorldDeltaHeaderSize + 1)], out var header, out var poses, out var sectors, out var consumed, out _));
        Assert.Equal(99u, header.GameTic);
        Assert.Empty(poses);
        Assert.Empty(sectors);
        Assert.Equal(LiveConstants.ServerWorldDeltaHeaderSize + 1, consumed);
    }

    [Fact]
    public void SinglePose_RoundTrip()
    {
        var pose = new PlayerPoseWorldDelta(
            playerNum: 0,
            flags: LiveConstants.ServerWorldDeltaPoseHasActor | LiveConstants.ServerWorldDeltaPoseLive,
            health: 100,
            armor: 50,
            posX: 128.5f,
            posY: -64.25f,
            posZ: 32f,
            velX: 1f,
            velY: 2f,
            velZ: 0f,
            yawBams: 0x40000000,
            pitchBams: 0);

        Span<byte> chunk = stackalloc byte[64];
        var written = WorldDeltaChunkCodec.Write(chunk, flags: 0, gameTic: 5, new[] { pose }, ReadOnlySpan<SectorWorldDelta>.Empty);
        Assert.True(written > 0);

        Assert.True(WorldDeltaChunkCodec.TryRead(chunk[..written], out _, out var poses, out _, out var consumed, out _));
        Assert.Equal(written, consumed);
        Assert.Single(poses);
        Assert.Equal(pose.Health, poses[0].Health);
        Assert.Equal(pose.PosX, poses[0].PosX);
        Assert.Equal(pose.YawBams, poses[0].YawBams);
    }
}

public class ServerSnapshotTailCodecTests
{
    [Fact]
    public void MinimalTail_HasEmptyHcdwAndHcdaBlocks()
    {
        Span<byte> tail = stackalloc byte[ServerSnapshotTailCodec.MinimalTailSize];
        Assert.Equal(ServerSnapshotTailCodec.MinimalTailSize, ServerSnapshotTailCodec.WriteMinimal(tail, gameTic: 12));

        Assert.True(ServerSnapshotTailCodec.TryReadMinimal(tail, out var worldDelta, out var actorDelta, out var consumed, out _));
        Assert.Equal(12u, worldDelta.GameTic);
        Assert.Equal(LiveConstants.ActorDeltasFlagComplete, actorDelta.Flags);
        Assert.Equal((byte)0, actorDelta.RecordCount);
        Assert.Equal(tail.Length, consumed);
    }

    [Fact]
    public void BuildServerSnapshot_WithMinimalTail_AppendsTailAfterHcsr()
    {
        Span<byte> payload = stackalloc byte[512];
        var command = new UserCmd(1, 0, 90, 0, 0, 0, 0);
        var players = new[]
        {
            new ServerSnapshotPlayerRecord
            {
                PlayerNum = 0,
                Commands = new[]
                {
                    new ServerSnapshotCommandRecord
                    {
                        CommandOffset = 0,
                        Command = command,
                    },
                },
            },
        };

        var written = GameplayPayloadBuilders.BuildServerSnapshot(
            payload,
            playerCount: 1,
            commandTics: 1,
            consistencyTics: 0,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 1,
            baseConsistency: 0,
            players,
            includeMinimalTail: true,
            gameTic: 44);

        Assert.True(written > LiveConstants.ServerSnapshotHeaderSize);
        Assert.True(ServerSnapshotHeader.TryRead(payload[..written], out var header));
        Assert.True(header.BodyBytes > LiveConstants.ServerSnapshotRecordsHeaderSize);

        Assert.True(ServerSnapshotBodyCodec.TryReadPlayerRecords(
            payload[LiveConstants.ServerSnapshotHeaderSize..written],
            header.ConsistencyTics,
            header.CommandTics,
            out _,
            out var hcsrBytes,
            out _));

        var tail = payload[(LiveConstants.ServerSnapshotHeaderSize + hcsrBytes)..written];
        Assert.True(ServerSnapshotTailCodec.TryReadMinimal(tail, out var worldDelta, out _, out _, out _));
        Assert.Equal(44u, worldDelta.GameTic);
    }
}
