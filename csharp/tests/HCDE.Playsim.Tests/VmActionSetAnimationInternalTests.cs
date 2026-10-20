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
}
