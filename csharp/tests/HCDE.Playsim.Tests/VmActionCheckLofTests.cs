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
    public void ResolveRange_PlayerUsesPlayerMissileRange()
    {
        Assert.Equal(VmActionCheckLof.PlayerMissileRange, VmActionCheckLof.ResolveRange(0, true));
    }

    [Fact]
    public void AdjustPitch_AimVertNoOffset_UsesHeightInDelta()
    {
        var withOffset = VmActionCheckLof.AdjustPitchRadians(
            false, true, 0, 0, 100, 50, 10, 5);
        var plain = VmActionCheckLof.AdjustPitchRadians(
            false, false, 0, 0, 100, 50, 10, 5);
        Assert.NotEqual(withOffset, plain);
    }

    [Fact]
    public void TraceSuccess_JumpOnMiss_AllowsNonActorHit()
    {
        Assert.True(VmActionCheckLof.TraceSuccess(false, VmActionCheckLof.CloffJumpOnMiss, false, 3));
    }
}
