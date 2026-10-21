namespace HCDE.Playsim.Tests;

public class VmActionSetAnimationNativesTests
{
    [Fact]
    public void GetObjectToWorldMatrixRawNative_FillsWhenOk()
    {
        VmActionSetAnimationNatives.GetObjectToWorldMatrixRawNative(true, true, out var fill);
        Assert.True(fill);
    }

    [Fact]
    public void SetAnimationNative_UsesTicFracOne()
    {
        VmActionSetAnimationNatives.SetAnimationNative(0, 30, 0, 0, -1, 0, 0, out var tics, out var frac);
        Assert.Equal(1, tics);
        Assert.Equal(1.0, frac);
    }

    [Fact]
    public void SetAnimationFrameRateInternalNative_RejectsNegativeRate()
    {
        Assert.True(VmActionSetAnimationNatives.SetAnimationFrameRateInternalNative(
            false, 1, true, -1, 1, false, false, 1, 0, 0,
            out _, out var neg, out _, out _, out _));
        Assert.True(neg);
    }

    [Fact]
    public void AnimLayerToAnimInfoNative_NoneWhenNoLayer()
    {
        Assert.False(VmActionSetAnimationNatives.AnimLayerToAnimInfoNative(
            false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, out var flags, out _));
        Assert.Equal(VmActionSetAnimationInternal.ModelAnimNone, flags);
    }
}
