namespace HCDE.Net.Core.Tests;

public class ServerSnapshotBodyCodecTests
{
    [Fact]
    public void SinglePlayerSingleCommand_RoundTrip()
    {
        Span<byte> payload = stackalloc byte[256];
        var command = new UserCmd(2, 0, 180, 0, -5, 3, 0);
        var written = GameplayPayloadBuilders.BuildServerSnapshotSinglePlayer(payload, playerNum: 1, command, averageLatency: 42);
        Assert.True(written > LiveConstants.ServerSnapshotHeaderSize);

        Assert.True(ServerSnapshotHeader.TryRead(payload[..written], out var header));
        Assert.True(ServerSnapshotBodyCodec.TryReadPlayerRecords(
            payload[LiveConstants.ServerSnapshotHeaderSize..written],
            header.ConsistencyTics,
            header.CommandTics,
            out var players,
            out var consumed,
            out _));
        Assert.Equal(written - LiveConstants.ServerSnapshotHeaderSize, consumed);
        Assert.Single(players);
        Assert.Equal((ushort)42, players[0].AverageLatency);
        Assert.Equal(command.Yaw, players[0].Commands[0].Command.Yaw);
    }
}
