namespace HCDE.Net.Core.Tests;

public class VmINetHostCheckConnectionsTests
{
    [Fact]
    public void HostCheckForConnectionsNative_ReturnsReadyWhenFull()
    {
        var ctx = new HostCheckForConnectionsContext
        {
            ConnectedPlayers = 4,
            MaxClients = 4,
            NetBuffer = new byte[8],
            GetKickClients = () => [],
            GetBanClients = () => [],
            GetConnection = () => default,
            GetStatus = _ => 3,
            CstatReady = 3,
            CstatNone = 0,
            HasPendingReliableService = _ => false,
        };
        Assert.True(VmINetHostCheckConnections.HostCheckForConnectionsNative(ctx));
    }

    [Fact]
    public void HostCheckForConnectionsNative_RemovesKickedClient()
    {
        var removed = new List<int>();
        var ctx = new HostCheckForConnectionsContext
        {
            ConnectedPlayers = 2,
            MaxClients = 4,
            MaxPlayers = 8,
            NetBuffer = new byte[8],
            GetKickClients = () => [1],
            GetBanClients = () => [],
            GetStatus = i => i == 1 ? (byte)1 : (byte)0,
            CstatNone = 0,
            GetClientAddress = _ => (0, 0),
            RemoveClientConnection = (c, _) => removed.Add(c),
            UpdatePlayers = (_, _) => { },
            RejectConnection = (_, _) => { },
            GetConnection = () => default,
            HasPendingReliableService = _ => false,
        };
        VmINetHostCheckConnections.HostCheckForConnectionsNative(ctx);
        Assert.Equal([1], removed);
        Assert.Equal(1, ctx.ConnectedPlayers);
    }
}
