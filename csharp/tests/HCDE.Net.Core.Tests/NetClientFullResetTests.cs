using HCDE.Net.Core;

namespace HCDE.Net.Core.Tests;

public class NetClientFullResetTests
{
    [Fact]
    public void ChannelReset_ClearsLatencyAndConsistencyArrays()
    {
        var channel = new NetClientChannelResetState
        {
            CurrentLatency = 99,
            MalformedPacketStrikes = 3,
            LastVerifiedConsistency = 12,
        };
        channel.NetConsistency[0] = 7;
        channel.Reset();
        Assert.Equal(0u, channel.CurrentLatency);
        Assert.Equal(0, channel.MalformedPacketStrikes);
        Assert.Equal(-1, channel.LastVerifiedConsistency);
        Assert.All(channel.NetConsistency, value => Assert.Equal(0, value));
    }

    [Fact]
    public void DisconnectTeardown_MarksPlayerGoneAndResetsChannel()
    {
        var registry = new LivePeerNetRegistry(4);
        var session = new NetClientDisconnectSession(registry);
        var teardown = new NetClientDisconnectTeardown(session);
        session.MarkInGame(1);
        teardown.Channel(1).CurrentLatency = 50;
        Assert.True(teardown.TryDisconnect(1, "test"));
        Assert.Equal(NetPlayerLifecycleState.Gone, teardown.Player(1).Lifecycle);
        Assert.Equal(0u, teardown.Channel(1).CurrentLatency);
        Assert.False(session.IsInGame(1));
    }

    [Fact]
    public void ResetClientChannel_ClearsLateJoinAndChannelState()
    {
        var registry = new LivePeerNetRegistry(4);
        var session = new NetClientDisconnectSession(registry);
        var teardown = new NetClientDisconnectTeardown(session);
        session.LateJoin.SetLateJoinSyncPending(2, 10, 3, 0);
        teardown.Channel(2).PassiveResendLastSequenceAck = 4;
        teardown.ResetClientChannel(2);
        Assert.False(session.LateJoin.IsLateJoinSyncPending(2));
        Assert.Equal(-1, teardown.Channel(2).PassiveResendLastSequenceAck);
    }
}
