namespace HCDE.Net.Core.Tests;

public class VmDNetTryRunTicsStallTests
{
    [Fact]
    public void ShouldSuppressTicGateStallWarningNative_WhenLeadHeld() =>
        Assert.True(VmDNetTryRunTicsStall.ShouldSuppressTicGateStallWarningNative(
            predictionLeadHeldWorld: true,
            netgame: true,
            demoPlayback: false,
            levelStartStatus: 0,
            levelStartDelay: 0,
            dedicatedClient: false,
            availableTics: 0,
            predictionLead: 2,
            clientTic: 20,
            gameTic: 10,
            ticDup: 1));

    [Fact]
    public void IsSustainedDedicatedTicGateStallNative_DetectsBacklog() =>
        Assert.True(VmDNetTryRunTicsStall.IsSustainedDedicatedTicGateStallNative(
            availableTics: 0, commandBacklog: 10, enterTic: 100, lastGameUpdate: 0, ticRate: 35, ticDup: 1));

    [Fact]
    public void ShouldEmitTicGateStallWarningNative_WhenIntervalElapsed() =>
        Assert.True(VmDNetTryRunTicsStall.ShouldEmitTicGateStallWarningNative(
            suppress: false, netgame: true, totalTics: 1, enterTic: 70, lastTicGateStallTrace: 0, ticRate: 35));

    [Fact]
    public void ShouldReportPredictionFaultNative_WhenStaleAndInput() =>
        Assert.True(VmDNetTryRunTicsStall.ShouldReportPredictionFaultNative(
            netgame: true, totalTics: 1, invasionAuthority: false, enterTic: 10, pauseGraceUntil: 0,
            staleAuthorityGate: true, wantsAttack: true, wantsMove: false, compatPredicting: false));

    [Fact]
    public void WantsMoveFromCmdNative_DetectsSideMove() =>
        Assert.True(VmDNetTryRunTicsStall.WantsMoveFromCmdNative(0, 1, 0));

    [Fact]
    public void AccumulateInvasionMirrorCountsNative_SplitsProjectile() {
        var live = 0; var blocking = 0; var projectile = 0;
        VmDNetTryRunTicsStall.AccumulateInvasionMirrorCountsNative(true, ref live, ref blocking, ref projectile);
        Assert.Equal(1, live);
        Assert.Equal(1, projectile);
        Assert.Equal(0, blocking);
    }
}
