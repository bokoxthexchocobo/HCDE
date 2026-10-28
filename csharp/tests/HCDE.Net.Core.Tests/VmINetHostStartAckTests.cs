namespace HCDE.Net.Core.Tests;

public class VmINetHostStartAckTests
{
    [Fact]
    public void SendAbortNative_AuthoritySendsTwoByteExit()
    {
        var sent = new List<int>();
        var ctx = new SendAbortContext
        {
            NetBuffer = new byte[4],
            ConsolePlayer = 0,
            MaxClients = 4,
            NcmdExit = 4,
            CstatNone = 0,
            GetStatus = i => i == 1 ? (byte)1 : (byte)0,
            SendPacket = sent.Add,
        };
        VmINetHostStartAck.SendAbortNative(ctx);
        Assert.Equal(2, ctx.NetBufferLength);
        Assert.Single(sent);
    }

    [Fact]
    public void HostCheckStartGameAcksNative_AllAcked()
    {
        var ctx = new HostCheckStartGameAcksContext
        {
            ConnectedPlayers = 2,
            MaxClients = 4,
            GetConnection = () => default,
            GetStatus = i => i == 0 ? (byte)0 : (byte)1,
            CstatNone = 0,
            HasStartGameAck = _ => true,
            UpdatePlayers = (_, _) => { },
        };
        Assert.True(VmINetHostStartAck.HostCheckStartGameAcksNative(ctx));
    }

    [Fact]
    public void HostGameThroughNetMessageNative_SinglePlayerShortcut()
    {
        var multiplayer = false;
        var maxClients = 1;
        var ctx = new HostGameContext
        {
            Arg = 1,
            MaxClients = maxClients,
            MaxPlayers = 16,
            NumArgs = () => 2,
            GetArg = _ => "1",
            SetMaxClients = v => maxClients = v,
            TryParseClientCount = s => VmINetNetCmdSetup.TryParseStrictIntNative(s, out var v) ? (true, v) : (false, 0),
            SetTicDup = _ => { },
            SetMultiplayer = v => multiplayer = v,
        };
        Assert.True(VmINetHostStartAck.HostGameThroughNetMessageNative(ctx));
        Assert.True(multiplayer);
    }
}
