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
}
