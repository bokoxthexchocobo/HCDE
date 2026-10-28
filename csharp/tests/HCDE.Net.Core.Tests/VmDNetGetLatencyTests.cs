namespace HCDE.Net.Core.Tests;

public class VmDNetGetLatencyTests
{
    [Fact]
    public void LatencySeverityNative_Tier120() =>
        Assert.Equal(2, VmDNetGetLatency.LatencySeverityNative(125));

    [Fact]
    public void GameDelayMsNative_UsesTicRate() =>
        Assert.Equal(142, VmDNetGetLatency.GameDelayMsNative(35, 30, 35));

}
