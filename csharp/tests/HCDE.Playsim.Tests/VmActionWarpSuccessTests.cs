namespace HCDE.Playsim.Tests;

public class VmActionWarpSuccessTests
{
    [Fact]
    public void ShouldSetSuccessStatePointer_WhenWarpOk() =>
        Assert.True(VmActionWarpSuccess.ShouldSetSuccessStatePointer(true, new object(), true));

    [Fact]
    public void ShouldSetChainSuccess_WhenNoStateJump() =>
        Assert.True(VmActionWarpSuccess.ShouldSetChainSuccess(true, null, true));
}
