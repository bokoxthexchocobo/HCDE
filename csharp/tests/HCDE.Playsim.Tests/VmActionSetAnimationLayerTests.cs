namespace HCDE.Playsim.Tests;

public class VmActionSetAnimationLayerTests
{
    [Fact]
    public void SetAnimationLayerAnimationNative_RestoresOnFailure()
    {
        Assert.False(VmActionSetAnimationLayer.SetAnimationLayerAnimationNative(
            false, true, false, out var restore, out _));
        Assert.False(VmActionSetAnimationLayer.SetAnimationLayerAnimationNative(
            true, false, false, out restore, out _));
        Assert.True(restore);
    }

    [Fact]
    public void SetAnimationLayerFrameRateNative_CopiesPrecalcFrame()
    {
        Assert.True(VmActionSetAnimationLayer.SetAnimationLayerFrameRateNative(
            true, true, true, out var copy));
        Assert.True(copy);
    }
}
