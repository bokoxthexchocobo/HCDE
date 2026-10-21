namespace HCDE.Net.Core.Tests;

public class VmDNetRuntimeClientConnectTests
{
    [Fact]
    public void ShouldIgnoreClientConnecting_WhenSetupInProgress() =>
        Assert.True(VmDNetRuntimeClientConnect.ShouldIgnoreClientConnecting(true, 1, 8, true, false, true));

    [Fact]
    public void SeedAppliedSequence_SetsBehindCurrent()
    {
        Assert.Equal(4, VmDNetRuntimeClientConnect.SeedAppliedSequence(5, -1));
    }

    [Fact]
    public void EnsureRuntimeSlotAddsClient_AppendsOnce()
    {
        var clients = new List<int>();
        Assert.True(VmDNetRuntimeClientConnect.EnsureRuntimeSlotAddsClient(false, 2, clients));
        Assert.Equal([2], clients);
    }
}
