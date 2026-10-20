namespace HCDE.Client.Tests;

public class VmDMainFragLimitPolicyTests
{
    [Fact]
    public void ShouldCheckFragLimit_RequiresDeathmatch()
    {
        Assert.False(VmDMainFragLimitPolicy.ShouldCheckFragLimit(false, 10));
        Assert.True(VmDMainFragLimitPolicy.ShouldCheckFragLimit(true, 10));
    }

    [Fact]
    public void ShouldExitLevel_WhenAtOrAboveLimit()
    {
        Assert.True(VmDMainFragLimitPolicy.ShouldExitLevel(20, 20));
        Assert.False(VmDMainFragLimitPolicy.ShouldExitLevel(20, 19));
    }
}
