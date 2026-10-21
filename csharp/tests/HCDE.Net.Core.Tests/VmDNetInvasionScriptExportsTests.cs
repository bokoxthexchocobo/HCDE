namespace HCDE.Net.Core.Tests;

public class VmDNetInvasionScriptExportsTests
{
    [Fact]
    public void InvasionBossWaveThunkNative_ReturnsOne() =>
        Assert.Equal(1, VmDNetInvasionScriptExports.InvasionIsBossWaveThunkNative(
            VmDNetInvasionGetters.InvWavefBoss, VmDNetInvasionGetters.InvWavefBoss));

    [Fact]
    public void InvasionControlThunkNative_ReturnsOne() =>
        Assert.Equal(1, VmDNetInvasionScriptExports.InvasionControlThunkNative(
            action: 1, invctlNextWave: 1, invctlVictory: 2, invctlFailure: 3,
            invasionAuthority: true, invasionModeEnabled: true));
}
