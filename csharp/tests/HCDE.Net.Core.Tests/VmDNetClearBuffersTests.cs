namespace HCDE.Net.Core.Tests;

public class VmDNetClearBuffersTests
{
    [Fact]
    public void NextInvasionStateTics_DecrementsWhenActive()
    {
        Assert.Equal(2, VmDNetClearBuffers.NextInvasionStateTics(3));
        Assert.Equal(0, VmDNetClearBuffers.NextInvasionStateTics(0));
    }

    [Fact]
    public void ShouldDecrementInvasionStateTics_RequiresNonAuthority()
    {
        Assert.True(VmDNetClearBuffers.ShouldDecrementInvasionStateTics(localInvasionAuthority: false, invasionDisabled: false));
        Assert.False(VmDNetClearBuffers.ShouldDecrementInvasionStateTics(localInvasionAuthority: true, invasionDisabled: false));
    }

    [Fact]
    public void ShouldSetConsolePlayerInGameOnClear_SkipsDedicatedAuthoritySlot()
    {
        Assert.False(VmDNetClearBuffers.ShouldSetConsolePlayerInGameOnClear(serverReservedSlot0: true));
        Assert.True(VmDNetClearBuffers.ShouldSetConsolePlayerInGameOnClear(serverReservedSlot0: false));
    }

    [Fact]
    public void DefaultClientSlotClearPlan_MatchesNativeZeros()
    {
        var plan = VmDNetClearBuffers.DefaultClientSlotClearPlan();
        Assert.False(plan.PlayingInGame);
        Assert.Equal(-1, plan.AppliedSequence);
        Assert.Equal(0u, plan.AuthorityEventReplayNextId);
    }

    [Fact]
    public void DefaultGlobalNetClearPlan_ClearsSessionFlags()
    {
        var plan = VmDNetClearBuffers.DefaultGlobalNetClearPlan(maxSendTics: 3);
        Assert.False(plan.NetGame);
        Assert.False(plan.PendingLocalHealthRepairValid);
        Assert.Equal(9, plan.FullLatencyCycle);
        Assert.Equal(int.MaxValue, plan.PredictionEndCapTic);
    }

    [Fact]
    public void IsTicGateClient_ExcludesLateJoinPending()
    {
        Assert.False(VmDNetClearBuffers.IsTicGateClient(1, 8, true, false, lateJoinPending: true));
        Assert.True(VmDNetClearBuffers.IsTicGateClient(1, 8, true, false, lateJoinPending: false));
    }

    [Fact]
    public void IsLateJoinSyncPending_MatchesBitmask()
    {
        Assert.True(VmDNetClearBuffers.IsLateJoinSyncPending(2, 8, 1ul << 2));
        Assert.False(VmDNetClearBuffers.IsLateJoinSyncPending(2, 8, 1ul << 1));
    }

    [Fact]
    public void IsCutsceneReadyParticipant_MatchesNativeGates()
    {
        Assert.True(VmDNetClearBuffers.IsCutsceneReadyParticipant(1, 8, true, false, false));
        Assert.False(VmDNetClearBuffers.IsCutsceneReadyParticipant(1, 8, true, true, false));
    }
}
