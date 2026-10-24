namespace HCDE.Net.Core.Tests;

public class VmDNetTryRunTicsSchedulerTests
{
    [Fact]
    public void ClampSingleticsTicsNative_LimitsToOne() =>
        Assert.Equal(1, VmDNetTryRunTicsScheduler.ClampSingleticsTicsNative(5, singletics: true));

    [Fact]
    public void ComputeAvailableTicsClientNative_AddsDedicatedBonus() =>
        Assert.Equal(4, VmDNetTryRunTicsScheduler.ComputeAvailableTicsClientNative(
            lowestSequence: 10, gameTic: 8, ticDup: 1, dedicatedClient: true, predictionLead: 2, clientTic: 12));

    [Fact]
    public void ApplyPredictionLeadCapNative_CapsRunTics() =>
        Assert.Equal(3, VmDNetTryRunTicsScheduler.ApplyPredictionLeadCapNative(
            runTics: 4, clientTic: 15, gameTic: 10, ticDup: 1, desiredLead: 2));

    [Fact]
    public void ReseatLateJoinClocksNative_AdvancesGametic() =>
        Assert.Equal((35, 40), VmDNetTryRunTicsScheduler.ReseatLateJoinClocksNative(
            lowestSequence: 7, ticDup: 5, desiredLead: 1, gameTic: 10, clientTic: 30));
}
