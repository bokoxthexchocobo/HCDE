namespace HCDE.Playsim.Tests;

public class VmActionFindAnimationFrameTests
{
    [Fact]
    public void UiFindTic_AddsFracWhenRunning() =>
        Assert.Equal(2.5, VmActionFindAnimationFrame.UiFindTic(2.0, 0.5, pausedOrFrozen: false));

    [Fact]
    public void BlendSizesMatch_RequiresEqual() =>
        Assert.True(VmActionFindAnimationFrame.BlendSizesMatch(3, 3));

    [Fact]
    public void BlendOneMinusT_Complements() =>
        Assert.Equal(0.2, VmActionFindAnimationFrame.BlendOneMinusT(0.8));

    [Fact]
    public void ThrowWhenBlendSizesDiffer_FlagsMismatch() =>
        Assert.True(VmActionFindAnimationFrame.ThrowWhenBlendSizesDiffer(match: false));

    [Fact]
    public void SetRetInterWhenThird() =>
        Assert.True(VmActionFindAnimationFrame.SetRetInterWhenThird(numretGt2: true));

    [Fact]
    public void ShouldInterpolateBone_InRange() =>
        Assert.True(VmActionFindAnimationFrame.ShouldInterpolateBone(index: 2, count: 5));

    [Fact]
    public void ResizeBlendOutput_WhenPositive() =>
        Assert.True(VmActionFindAnimationFrame.ResizeBlendOutput(size: 4));
}
