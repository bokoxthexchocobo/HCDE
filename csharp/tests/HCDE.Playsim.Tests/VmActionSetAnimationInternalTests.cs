namespace HCDE.Playsim.Tests;

public class VmActionSetAnimationInternalTests
{
    [Fact]
    public void SameAnimationBlocked_WhenNoOverrideAndSameStart() =>
        Assert.True(VmActionSetAnimationInternal.SameAnimationBlocked(
            VmActionSetAnimationInternal.SafNooverride, 1, 10, 10));

    [Fact]
    public void ResolveLastFrame_NegativeEndUsesAnimEnd() =>
        Assert.Equal(19, VmActionSetAnimationInternal.ResolveLastFrame(-1, 20, 5));

    [Fact]
    public void NormalizeInterpolateTics_MinOne() =>
        Assert.Equal(1, VmActionSetAnimationInternal.NormalizeInterpolateTics(0));

    [Fact]
    public void RunSetAnimationInternal_SuccessBuildsFrames()
    {
        var input = new VmActionSetAnimationInternal.SetAnimationGateInput(
            VmActionSetAnimationInternal.Mf9Decoupledanimations,
            true,
            "Run",
            0,
            -1,
            0,
            0,
            5,
            25,
            -1,
            -1,
            -1,
            0,
            30,
            30,
            100,
            0,
            false,
            false,
            2);
        var (exit, frames) = VmActionSetAnimationInternal.RunSetAnimationInternal(input);
        Assert.Equal(VmActionSetAnimationInternal.SetAnimationEarlyExit.Success, exit);
        Assert.Equal(24, frames.LastFrame);
        Assert.Equal(102, frames.StartTic);
    }

    [Fact]
    public void CheckAnimationFrameBounds_DetectsStart()
    {
        Assert.Equal(
            VmActionSetAnimationInternal.FrameBoundsViolation.Start,
            VmActionSetAnimationInternal.CheckAnimationFrameBounds(10, 0, 0, 5));
    }
}
