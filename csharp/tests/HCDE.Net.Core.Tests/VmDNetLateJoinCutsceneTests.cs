namespace HCDE.Net.Core.Tests;

public class VmDNetLateJoinCutsceneTests
{
    [Fact]
    public void SetLateJoinSyncPendingNative_ClampsTargets()
    {
        Assert.True(VmDNetLateJoinCutscene.SetLateJoinSyncPendingNative(
            0, 8, -5, -2, 1, out var seq, out var con, out var set));
        Assert.Equal(-1, seq);
        Assert.True(set);
    }

    [Fact]
    public void ClearLateJoinSyncPendingNative_SetsConsistencyGrace()
    {
        Assert.True(VmDNetLateJoinCutscene.ClearLateJoinSyncPendingNative(
            1, 8, 1ul << 1, gametic: 100, ticRate: 35, priorConsistencyGraceUntilTic: 50,
            out var clear, out var grace));
        Assert.True(clear);
        Assert.Equal(135, grace);
    }

    [Fact]
    public void RequestRuntimeResyncNative_SetsResendFromAck()
    {
        Assert.True(VmDNetLateJoinCutscene.RequestRuntimeResyncNative(
            true, 1, 8, false, true, sequenceAck: 4, consistencyAck: 9,
            out var seq, out var con, out var waiting, out var pending));
        Assert.Equal(5, seq);
        Assert.Equal(10, con);
        Assert.True(waiting);
        Assert.True(pending);
    }

    [Fact]
    public void GetCutsceneReadyHostNative_PrefersAuthority()
    {
        var host = VmDNetLateJoinCutscene.GetCutsceneReadyHostNative(0, true, new[] { 1, 2 }, c => c == 0);
        Assert.Equal(0, host);
    }

    [Fact]
    public void PlayerReadiedUpToggleNative_ForcesReadyWhenSoloHuman()
    {
        Assert.True(VmDNetLateJoinCutscene.PlayerReadiedUpToggleNative(
            true, false, 0, 8, 10, 0, 1, 1, false, out var force, out _, out _));
        Assert.True(force);
    }
}
