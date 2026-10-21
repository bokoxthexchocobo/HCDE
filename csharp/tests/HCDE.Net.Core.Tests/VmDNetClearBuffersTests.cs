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
    public void ClientSlotClearDefaults_MatchNativeZeros()
    {
        Assert.False(VmDNetClearBuffers.ClientSlotPlayingInGameAfterClearNative());
        Assert.Equal(-1, VmDNetClearBuffers.ClientSlotAppliedSequenceAfterClearNative());
        Assert.Equal(0u, VmDNetClearBuffers.ClientSlotAuthorityEventReplayNextIdAfterClearNative());
    }

    [Fact]
    public void GlobalClearDefaults_MatchNativeSessionFlags()
    {
        Assert.False(VmDNetClearBuffers.GlobalNetGameAfterClearNative());
        Assert.False(VmDNetClearBuffers.GlobalPendingLocalHealthRepairValidAfterClearNative());
        Assert.Equal(9, VmDNetClearBuffers.GlobalFullLatencyCycleAfterClearNative(maxSendTics: 3));
        Assert.Equal(int.MaxValue, VmDNetClearBuffers.GlobalPredictionEndCapTicAfterClearNative());
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
