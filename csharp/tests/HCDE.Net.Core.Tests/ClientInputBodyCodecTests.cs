namespace HCDE.Net.Core.Tests;

public class ClientInputBodyCodecTests
{
    [Fact]
    public void SinglePlayerSingleCommand_RoundTrip()
    {
        Span<byte> payload = stackalloc byte[256];
        var command = new UserCmd(buttons: 1, pitch: 90, yaw: -45, roll: 0, forwardMove: 10, sideMove: 0, upMove: 0);
        var written = GameplayPayloadBuilders.BuildClientInputSinglePlayer(payload, playerNum: 1, command);
        Assert.True(written > LiveConstants.ClientInputHeaderSize);

        Assert.True(ClientInputHeader.TryRead(payload[..written], out var header));
        Assert.Equal((byte)1, header.PlayerCount);
        Assert.Equal((byte)1, header.CommandTics);
        Assert.True(ClientInputHeader.ValidateHeader(header, written, out _));

        Assert.True(ClientInputBodyCodec.TryRead(
            payload[LiveConstants.ClientInputHeaderSize..written],
            header.ConsistencyTics,
            header.CommandTics,
            out var players,
            out _));
        Assert.Single(players);
        Assert.Equal((byte)1, players[0].PlayerNum);
        Assert.Single(players[0].Commands);
        Assert.Equal(command.Buttons, players[0].Commands[0].Command.Buttons);
    }
}
