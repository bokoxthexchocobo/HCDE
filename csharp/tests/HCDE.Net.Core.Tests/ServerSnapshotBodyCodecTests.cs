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

    [Fact]
    public void MultiPlayerMultiTic_RoundTrip()
    {
        var players = new[]
        {
            CreatePlayerRecord(playerNum: 1, yaw: 90, latency: 10, commandTics: 2),
            CreatePlayerRecord(playerNum: 2, yaw: 180, latency: 20, commandTics: 2),
        };

        Span<byte> payload = stackalloc byte[1024];
        var written = GameplayPayloadBuilders.BuildServerSnapshot(
            payload,
            playerCount: 2,
            commandTics: 2,
            consistencyTics: 1,
            sequenceAck: 0,
            consistencyAck: 0,
            baseSequence: 1,
            baseConsistency: 0,
            players,
            includeMinimalTail: true,
            gameTic: 55);

        Assert.True(ServerSnapshotHeader.TryRead(payload[..written], out var header));
        Assert.Equal((byte)2, header.PlayerCount);
        Assert.True(ServerSnapshotBodyCodec.TryReadPlayerRecords(
            payload[LiveConstants.ServerSnapshotHeaderSize..written],
            header.ConsistencyTics,
            header.CommandTics,
            out var parsed,
            out _,
            out _));
        Assert.Equal(2, parsed.Count);
        Assert.Equal(90, parsed[0].Commands[0].Command.Yaw);
        Assert.Equal(180, parsed[1].Commands[0].Command.Yaw);
        Assert.Equal(2, parsed[0].Commands.Count);
        Assert.Equal(2, parsed[1].Commands.Count);
    }

    [Fact]
    public void DuplicateCommandOffset_IsRejected()
    {
        Span<byte> body = stackalloc byte[256];
        var written = ServerSnapshotBodyCodec.WritePlayerRecords(body, new[]
        {
            new ServerSnapshotPlayerRecord
            {
                PlayerNum = 0,
                Commands = new[]
                {
                    new ServerSnapshotCommandRecord { CommandOffset = 0, Command = UserCmd.Zero },
                    new ServerSnapshotCommandRecord { CommandOffset = 0, Command = UserCmd.Zero },
                },
            },
        });
        Assert.True(written > 0);

        Assert.False(ServerSnapshotBodyCodec.TryReadPlayerRecords(
            body[..written],
            expectedConsistencyTics: 0,
            expectedCommandTics: 2,
            out _,
            out _,
            out var reason));
        Assert.Equal("server-snapshot-duplicate-command-offset", reason);
    }

    private static ServerSnapshotPlayerRecord CreatePlayerRecord(byte playerNum, short yaw, ushort latency, byte commandTics)
    {
        var commands = new ServerSnapshotCommandRecord[commandTics];
        for (byte i = 0; i < commandTics; i++)
        {
            commands[i] = new ServerSnapshotCommandRecord
            {
                CommandOffset = i,
                Command = new UserCmd(1, 0, yaw, 0, 0, 0, 0),
            };
        }

        return new ServerSnapshotPlayerRecord
        {
            PlayerNum = playerNum,
            AverageLatency = latency,
            ConsistencyValues = new ushort[] { 1 },
            Commands = commands,
        };
    }
}
