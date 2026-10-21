namespace HCDE.Net.Core.Tests;

public class VmSvMasterPublicApiTests
{
    [Fact]
    public void ShouldShutdownWhenDisabledNative_WhenMastersOffButSocketOpen()
    {
        Assert.True(VmSvMasterPublicApi.ShouldShutdownWhenDisabledNative(svUseMasters: false, masterSocket: 3));
    }

    [Fact]
    public void ShouldReresolveAddressesNative_RespectsInterval()
    {
        Assert.True(VmSvMasterPublicApi.ShouldReresolveAddressesNative(10_000, 0, 1000, force: false));
        Assert.False(VmSvMasterPublicApi.ShouldReresolveAddressesNative(1500, 1000, 1000, force: false));
    }
}
