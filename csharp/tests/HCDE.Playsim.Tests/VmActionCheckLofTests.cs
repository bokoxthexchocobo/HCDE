namespace HCDE.Playsim.Tests;

public class VmActionCheckLofTests
{
    [Fact]
    public void TargetNullWithoutAllow_FailsWhenStrict()
    {
        Assert.True(VmActionCheckLof.TargetNullWithoutAllow(0, false));
        Assert.False(VmActionCheckLof.TargetNullWithoutAllow(VmActionCheckLof.CloffAllownull, false));
    }

    [Fact]
    public void RangeExceeded_IgnoresPartialFlag()
    {
        Assert.True(VmActionCheckLof.RangeExceeded(100, 50, false));
        Assert.False(VmActionCheckLof.RangeExceeded(100, 50, true));
    }

    [Fact]
    public void DefaultRange_PlayerUsesPlayerMissileRange()
    {
        Assert.Equal(VmActionCheckLof.PlayerMissileRange, VmActionCheckLof.DefaultRange(true, VmActionCheckLof.PlayerMissileRange, 1));
    }
}
