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
    public void CalculateAnimationFrameNullIframe_Aborts() =>
        Assert.True(VmActionCalculateAnimation.CalculateAnimationFrameNullIframe(iframeNull: true));

    [Fact]
    public void ReturnNullWhenModelMissing() =>
        Assert.True(VmActionCalculateAnimation.ReturnNullWhenModelMissing(hasModel: false));

    [Fact]
    public void RestoreAnimLayerAfterCalculate_WhenLayerPresent() =>
        Assert.True(VmActionCalculateAnimation.RestoreAnimLayerAfterCalculate(layerNonNull: true));

    [Fact]
    public void UiModelTimer_AddsFracWhenUnpaused() =>
        Assert.Equal(5.5, VmActionCalculateAnimation.UiModelTimer(5.0, 0.5, pausedOrFrozen: false));

    [Fact]
    public void FindAnimationFrameAtSetsFloat_WhenThirdRet() =>
        Assert.True(VmActionCalculateAnimation.FindAnimationFrameAtSetsFloat(numretGt2: true));

    [Fact]
    public void FindAnimationFrameAtSetsObject_WhenSecondRet() =>
        Assert.True(VmActionCalculateAnimation.FindAnimationFrameAtSetsObject(numretGt1: true));

    [Fact]
    public void CalcUsesModelTimerPlusOne() =>
        Assert.True(VmActionCalculateAnimation.CalcUsesModelTimerPlusOne(timer: 0));

    [Fact]
    public void UiFindTic_AddsFracWhenRunning() =>
        Assert.Equal(2.5, VmActionCalculateAnimation.UiFindTic(2.0, 0.5, pausedOrFrozen: false));

    [Fact]
    public void SetRetInterWhenThird() =>
        Assert.True(VmActionCalculateAnimation.SetRetInterWhenThird(numretGt2: true));

    [Fact]
    public void BlendSizesMatch_RequiresEqual() =>
        Assert.True(VmActionCalculateAnimation.BlendSizesMatch(4, 4));

    [Fact]
    public void BlendOneMinusT_Complements() =>
        Assert.InRange(VmActionCalculateAnimation.BlendOneMinusT(0.8), 0.199, 0.201);

    [Fact]
    public void ThrowWhenBlendSizesDiffer_FlagsMismatch() =>
        Assert.True(VmActionCalculateAnimation.ThrowWhenBlendSizesDiffer(match: false));

    [Fact]
    public void ShouldInterpolateBone_InRange() =>
        Assert.True(VmActionCalculateAnimation.ShouldInterpolateBone(index: 2, count: 5));

    [Fact]
    public void OffsetSizesMatch() =>
        Assert.True(VmActionCalculateAnimation.OffsetSizesMatch(4, 4));

    [Fact]
    public void ThrowOffsetSizeMismatch() =>
        Assert.True(VmActionCalculateAnimation.ThrowOffsetSizeMismatch(match: false));

    [Fact]
    public void NeedsBoneOverrideSlot_WhenEmpty() =>
        Assert.True(VmActionCalculateAnimation.NeedsBoneOverrideSlot(overrideSize: 0));

    [Fact]
    public void MinBoneLoopCount_TakesMinimum() =>
        Assert.Equal(3, VmActionCalculateAnimation.MinBoneLoopCount(frameCount: 3, overrideCount: 10));

    [Fact]
    public void SetBonesUiTic_UsesFrac() =>
        Assert.Equal(3.25, VmActionCalculateAnimation.SetBonesUiTic(3.0, 0.25));

    [Fact]
    public void ShouldCalcBonesAfterSet_WhenAnyBone() =>
        Assert.True(VmActionCalculateAnimation.ShouldCalcBonesAfterSet(n: 1));

    [Fact]
    public void UiSkipsCalcBonesComment() =>
        Assert.True(VmActionCalculateAnimation.UiSkipsCalcBonesComment());
}
