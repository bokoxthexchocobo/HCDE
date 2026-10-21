namespace HCDE.Net.Core.Tests;

public class VmDNetScreenJobReadyTests
{
    [Fact]
    public void IsPlayerReadyParticipantNative_RequiresNoBot() =>
        Assert.True(VmDNetScreenJobReady.IsPlayerReadyParticipantNative(cutsceneReady: true, hasBot: false));

    [Fact]
    public void InvasionControlRejectedNative_WhenZero() =>
        Assert.True(VmDNetScreenJobReady.InvasionControlRejectedNative(0));
}
