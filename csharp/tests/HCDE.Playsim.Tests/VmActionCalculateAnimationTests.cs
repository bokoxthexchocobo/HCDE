namespace HCDE.Playsim.Tests;

public class VmActionCalculateAnimationTests
{
    [Fact]
    public void CalculateAnimationLayerNull_ReturnsNull() =>
        Assert.True(VmActionCalculateAnimation.CalculateAnimationLayerNull(layerNull: true));

    [Fact]
    public void CalculateAnimationInternalNullModel_WhenMissing() =>
        Assert.True(VmActionCalculateAnimation.CalculateAnimationInternalNullModel(modelFound: false));

    [Fact]
    public void FindAnimationFrameNegativeTic_SkipsWork() =>
        Assert.True(VmActionCalculateAnimation.FindAnimationFrameNegativeTic(-1));

    [Fact]
    public void ShouldCreateInterpFrame_WhenLayerAndTicOk() =>
        Assert.True(VmActionCalculateAnimation.ShouldCreateInterpFrame(layerNotNull: true, tic: 1));

    [Fact]
    public void BlendFrameSizesMatch_RequiresEqual() =>
        Assert.True(VmActionCalculateAnimation.BlendFrameSizesMatch(4, 4));

    [Fact]
    public void BlendInverseT_Complements() =>
        Assert.Equal(0.25, VmActionCalculateAnimation.BlendInverseT(0.75));

    [Fact]
    public void CalculateAnimationFrameNullIframe_Aborts() =>
        Assert.True(VmActionCalculateAnimation.CalculateAnimationFrameNullIframe(iframeNull: true));

    [Fact]
    public void FindAnimationFrameAtSetsFloat_WhenThirdRet() =>
        Assert.True(VmActionCalculateAnimation.FindAnimationFrameAtSetsFloat(numretGreaterThan2: true));

    [Fact]
    public void RestoreAnimLayerAfterCalculate_WhenLayerPresent() =>
        Assert.True(VmActionCalculateAnimation.RestoreAnimLayerAfterCalculate(layerNonNull: true));

    [Fact]
    public void UiModelTimer_AddsFracWhenUnpaused() =>
        Assert.Equal(5.5, VmActionCalculateAnimation.UiModelTimer(5.0, 0.5, pausedOrFrozen: false));

    [Fact]
    public void ReturnNullWhenModelMissing() =>
        Assert.True(VmActionCalculateAnimation.ReturnNullWhenModelMissing(hasModel: false));

    [Fact]
    public void CalcUsesModelTimerPlusOne() =>
        Assert.True(VmActionCalculateAnimation.CalcUsesModelTimerPlusOne(timer: 0));

    [Fact]
    public void FindAnimationFrameAtSetsObject_WhenSecondRet() =>
        Assert.True(VmActionCalculateAnimation.FindAnimationFrameAtSetsObject(numretGreaterThan1: true));
}
