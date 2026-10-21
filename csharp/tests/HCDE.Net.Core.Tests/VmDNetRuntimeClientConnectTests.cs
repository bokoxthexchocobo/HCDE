namespace HCDE.Net.Core.Tests;

public class VmDNetRuntimeClientConnectTests
{
    [Fact]
    public void ClientConnectingShouldReturnEarly_WhenSetupInProgress() =>
        Assert.True(VmDNetRuntimeClientConnect.ClientConnectingShouldReturnEarly(true, 1, 8, true, false, true));

    [Fact]
    public void AppliedSequenceAfterSeed_SetsBehindCurrent() =>
        Assert.Equal(4, VmDNetRuntimeClientConnect.AppliedSequenceAfterSeed(5, -1));

    [Fact]
    public void EnsureRuntimeSlotAddsClient_AppendsOnce()
    {
        var clients = new List<int>();
        Assert.True(VmDNetRuntimeClientConnect.EnsureRuntimeSlotAddsClient(false, 2, clients));
        Assert.Equal([2], clients);
    }

    [Fact]
    public void ClearRuntimeJoinStateNative_ClearsWhenPending()
    {
        var cleared = false;
        VmDNetRuntimeClientConnect.ClearRuntimeJoinStateNative(1ul << 2, 2, () => cleared = true, () => { });
        Assert.True(cleared);
    }

    [Fact]
    public void ApplyReconnectPresentationReset_InvokesAllSteps()
    {
        var n = 0;
        VmDNetRuntimeClientConnect.ApplyReconnectPresentationReset(
            () => n++,
            () => n++,
            () => n++,
            () => n++,
            () => n++,
            () => n++);
        Assert.Equal(6, n);
    }
}
