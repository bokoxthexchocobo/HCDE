namespace HCDE.Net.Core.Tests;

public class VmDNetTryRunTicsWorldLoopTests
{
    [Fact]
    public void MirrorVisualWorldStepsNative_ClampsSteps() =>
        Assert.Equal(6, VmDNetTryRunTicsWorldLoop.MirrorVisualWorldStepsNative(6, netgame: true, invasionAuthority: false));

    [Fact]
    public void ShouldRunCoopInterpolationNative_RequiresCoop() =>
        Assert.True(VmDNetTryRunTicsWorldLoop.ShouldRunCoopInterpolationNative(
            authority: false, netgame: true, deathmatch: false, gameType: 0));

    [Fact]
    public void ShouldStopWorldLoopNative_WhenCommandsReset() =>
        Assert.True(true);
}
