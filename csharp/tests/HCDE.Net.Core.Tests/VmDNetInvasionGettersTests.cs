namespace HCDE.Net.Core.Tests;

public class VmDNetInvasionGettersTests
{
    [Fact]
    public void ClassicInvasionStateNative_CountdownIsFive() =>
        Assert.Equal(5, VmDNetInvasionGetters.ClassicInvasionStateNative(1));

    [Fact]
    public void NonNegativeWaveNative_ClampsNegative() =>
        Assert.Equal(0, VmDNetInvasionGetters.NonNegativeWaveNative(-2));

    [Fact]
    public void IsBossWaveNative_DetectsFlag() =>
        Assert.True(VmDNetInvasionGetters.IsBossWaveNative(
            VmDNetInvasionGetters.InvWavefBoss, VmDNetInvasionGetters.InvWavefBoss));

    [Fact]
    public void SpawnSpotCountNative_ClampsNegative() =>
        Assert.Equal(0, VmDNetInvasionGetters.SpawnSpotCountNative(-1));

    [Fact]
    public void InvasionControlNextWaveNative_MatchesAction() =>
        Assert.Equal(1, VmDNetInvasionGetters.InvasionControlNextWaveNative(1, 1));

    [Fact]
    public void ArchvileMonsterEligibleNative_RequiresHealth() =>
        Assert.True(VmDNetInvasionGetters.ArchvileMonsterEligibleNative(5, 0, 0));
}
