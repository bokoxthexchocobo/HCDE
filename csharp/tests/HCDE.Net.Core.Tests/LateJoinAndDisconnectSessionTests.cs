using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class LateJoinAndDisconnectSessionTests
{
    [Fact]
    public void LateJoinSync_PendingUntilCleared()
    {
        var lateJoin = new LateJoinRuntimeSession();
        lateJoin.SetLateJoinSyncPending(2, 40, 8, 100, "runtime-connect");
        Assert.True(lateJoin.IsLateJoinSyncPending(2));
        Assert.Equal(40, lateJoin.TargetSequence(2));
        lateJoin.ClearLateJoinSyncPending(2, 200);
        Assert.False(lateJoin.IsLateJoinSyncPending(2));
    }

    [Fact]
    public void RuntimeBootstrap_SetsResendWindowAndPending()
    {
        var registry = new LivePeerNetRegistry(4);
        registry[1].SequenceAck = 10;
        var lateJoin = new LateJoinRuntimeSession();
        lateJoin.BeginRuntimeBootstrap(1, 70, 1, 5, 99, registry, "runtime-bootstrap");
        Assert.True(lateJoin.IsLateJoinSyncPending(1));
        Assert.True(registry[1].ResendSequenceFrom >= 0);
    }

    [Fact]
    public void Disconnect_IsIdempotentAndResetsRegistry()
    {
        var registry = new LivePeerNetRegistry(4);
        registry[2].CurrentSequence = 50;
        var disconnect = new NetClientDisconnectSession(registry);
        disconnect.MarkInGame(2);
        Assert.True(disconnect.TryDisconnect(2));
        Assert.Equal(0, registry[2].CurrentSequence);
        Assert.False(disconnect.TryDisconnect(2));
    }

    [Fact]
    public void ResetClientState_ClearsLateJoinAndCursors()
    {
        var registry = new LivePeerNetRegistry(4);
        var disconnect = new NetClientDisconnectSession(registry);
        disconnect.LateJoin.SetLateJoinSyncPending(3, 1, 1, 0);
        disconnect.ActorDeltaSendCursor[3] = 99;
        disconnect.ResetClientState(3);
        Assert.False(disconnect.LateJoin.IsLateJoinSyncPending(3));
        Assert.Equal(0u, disconnect.ActorDeltaSendCursor[3]);
    }
}
