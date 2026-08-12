using HCDE.Net.Transport;

namespace HCDE.Net.Core.Tests;

public class ServerSnapshotQuitterCodecTests
{
    [Fact]
    public void QuitterPrefix_RoundTrip()
    {
        var slots = new byte[] { 2, 5 };
        Span<byte> chunk = stackalloc byte[8];
        Assert.Equal(3, ServerSnapshotQuitterCodec.Write(chunk, slots));

        Assert.True(ServerSnapshotQuitterCodec.TryRead(chunk, quitterBytes: 3, out var parsed, out _));
        Assert.Equal(slots, parsed);
    }

    [Fact]
    public void BuildServerSnapshot_WithQuitters_InsertsPrefixBeforeHcsr()
    {
        Span<byte> payload = stackalloc byte[512];
        var command = new UserCmd(1, 0, 90, 0, 0, 0, 0);
        var players = new[]
        {
            new ServerSnapshotPlayerRecord
            {
                PlayerNum = 1,
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
            gameTic: 12,
            quitterPlayerSlots: new byte[] { 3 });

        Assert.True(ServerSnapshotHeader.TryRead(payload[..written], out var header));
        Assert.Equal((byte)NetCommandFlags.Quitters, header.ControlFlags);
        Assert.Equal((ushort)2, header.QuitterBytes);
        Assert.True(ServerSnapshotQuitterCodec.TryRead(
            payload[LiveConstants.ServerSnapshotHeaderSize..(LiveConstants.ServerSnapshotHeaderSize + header.QuitterBytes)],
            header.QuitterBytes,
            out var quitters,
            out _));
        Assert.Equal(new byte[] { 3 }, quitters);
        Assert.True(ServerSnapshotHeader.ValidateHeader(header, written, out _));
    }
}
