namespace HCDE.Net.Core.Tests;

public class VmSvMasterNms1LifecycleTests
{
    [Fact]
    public void ShouldReRegisterOnPortChangeNative_WhenStoredPortDiffers()
    {
        Assert.True(VmSvMasterNms1Lifecycle.ShouldReRegisterOnPortChangeNative(5029, 5030));
        Assert.False(VmSvMasterNms1Lifecycle.ShouldReRegisterOnPortChangeNative(0, 5030));
    }

    [Fact]
    public void ShouldSkipRegisterAttemptNative_RespectsCooldown()
    {
        var state = new MasterServerNms1State { LastRegisterAttempt = 1000 };
        Assert.True(VmSvMasterNms1Lifecycle.ShouldSkipRegisterAttemptNative(state, force: false, now: 20_000));
        Assert.False(VmSvMasterNms1Lifecycle.ShouldSkipRegisterAttemptNative(state, force: true, now: 20_000));
    }
}
