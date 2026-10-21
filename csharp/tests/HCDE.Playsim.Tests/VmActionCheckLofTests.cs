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
    public void EvaluateCheckLofTrace_SkipTargetBeyond_Continues()
    {
        var flags = VmActionCheckLof.CloffSkiptarget | VmActionCheckLof.CloffBeyondtarget;
        var (status, bad) = VmActionCheckLof.EvaluateCheckLofTrace(
            VmActionCheckLof.TraceHitActor, 7, 7, flags, false, 0, 0, 0, false, false);
        Assert.Equal(VmActionCheckLof.LofTraceStatus.Skip, status);
        Assert.False(bad);
    }

    [Fact]
    public void EvaluateCheckLofTrace_MustShootable_SkipsNonShootable()
    {
        var (status, _) = VmActionCheckLof.EvaluateCheckLofTrace(
            VmActionCheckLof.TraceHitActor, 2, 1, VmActionCheckLof.CloffMustbeshootable, false,
            0, 0, 0, false, false);
        Assert.Equal(VmActionCheckLof.LofTraceStatus.Skip, status);
    }
}
