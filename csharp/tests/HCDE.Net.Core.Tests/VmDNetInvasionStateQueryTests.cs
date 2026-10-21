namespace HCDE.Net.Core.Tests;

public class VmDNetInvasionStateQueryTests
{
    [Fact]
    public void NonNegativeWaveNative_ClampsNegative() =>
        Assert.Equal(0, VmDNetInvasionStateQuery.NonNegativeWaveNative(-3));

    [Fact]
    public void IsBossWaveNative_DetectsFlag() =>
        Assert.True(VmDNetInvasionStateQuery.IsBossWaveNative(
            VmDNetInvasionStateQuery.InvWavefBoss, VmDNetInvasionStateQuery.InvWavefBoss));

    [Fact]
    public void InvasionControlNextWaveNative_MatchesAction() =>
        Assert.Equal(1, VmDNetInvasionStateQuery.InvasionControlNextWaveNative(1, 1));

    [Fact]
    public void InvasionBossWaveAsIntNative_ReturnsOne() =>
        Assert.Equal(1, VmDNetInvasionStateQuery.InvasionBossWaveAsIntNative(true));

    [Fact]
    public void ArchvileActorEligibleNative_RequiresPositiveHealth() =>
        Assert.True(VmDNetInvasionStateQuery.ArchvileActorEligibleNative(10, 0, 0));
}
