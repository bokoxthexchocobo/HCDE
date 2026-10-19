using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class NetPresentationEchoResetTests
{
    [Fact]
    public void PresentationEchoRegistry_ResetClient_ClearsInitializedState()
    {
        var registry = new NetClientPresentationEchoRegistry(4);
        registry.CommitLastState(2, new PresentationEchoLastState([1, 2], [3], 4, 5, initialized: true));
        registry.ResetClient(2);
        Assert.False(registry.GetLastState(2).Initialized);
    }

    [Fact]
    public void DisconnectSession_ResetsPresentationEchoAndInterp()
    {
        var peers = new LivePeerNetRegistry(4);
        var echo = new NetClientPresentationEchoRegistry(4);
        var interp = new RemotePlayerInterpRegistry(4);
        var session = new NetClientDisconnectSession(peers, echo, interp);
        session.MarkInGame(1);
        echo.CommitLastState(1, new PresentationEchoLastState([9], [], 0, 0, true));
        interp[1].PushSample(10, 1, 2, 3, 4);
        Assert.True(session.TryDisconnect(1));
        Assert.False(echo.GetLastState(1).Initialized);
        Assert.Equal(0, interp[1].Count);
    }
}
