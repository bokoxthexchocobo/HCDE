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

    [Fact]
    public void FindFirstViolatingPlayer_ReturnsIndex()
    {
        var idx = VmDMainFragLimitPolicy.FindFirstViolatingPlayer(
            [true, false],
            [25, 5],
            20);
        Assert.Equal(0, idx);
    }
}
