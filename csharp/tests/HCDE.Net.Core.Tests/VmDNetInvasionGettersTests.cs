namespace HCDE.Net.Core.Tests;

public class VmDNetInvasionGettersTests
{
    [Fact]
    public void ClassicInvasionStateNative_CountdownIsFive() =>
        Assert.Equal(5, VmDNetInvasionGetters.ClassicInvasionStateNative(1));

    [Fact]
    public void NonNegativeWaveNative_ClampsNegative() =>
        Assert.Equal(0, VmDNetInvasionGetters.NonNegativeWaveNative(-3));

    [Fact]
    public void CountArchviles_SkipsIneligible()
    {
        var snaps = new[]
        {
            new ArchvileMonsterSnapshot(new object(), 10, 0, true),
            new ArchvileMonsterSnapshot(new object(), 0, 0, true),
        };
        Assert.Equal(1, VmDNetInvasionGetters.CountArchvilesAmongActiveMonstersNative(
            snaps, 0, archvileClassResolved: true));
    }

    [Fact]
    public void ResolveControlInvasion_RejectsWhenDisabled() =>
        Assert.Equal(0, VmDNetInvasionGetters.ResolveControlInvasionNative(
            invasionAuthority: false, invasionModeEnabled: true, action: 1, 1, 2, 3));
}
