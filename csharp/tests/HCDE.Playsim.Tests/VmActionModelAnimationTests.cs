namespace HCDE.Playsim.Tests;

public class VmActionModelAnimationTests
{
    [Fact]
    public void SkipSameAnimation_WhenNooverrideAndSameFrame() =>
        Assert.True(VmActionModelAnimation.SkipSameAnimation(VmActionModelAnimation.SafNooverride, curAnimNone: false, sameStartFrame: true));

    [Fact]
    public void AnimationLayerHasPointerFields() =>
        Assert.True(VmActionModelAnimation.AnimationLayerHasPointerFields(hasCurAnim: true, hasPrevAnim: false));

    [Fact]
    public void AnimationSequenceTracksLoopFrame() =>
        Assert.True(VmActionModelAnimation.AnimationSequenceTracksLoopFrame(loopFrame: 0));

    [Fact]
    public void HasBaseSpriteModelFrames_WhenKeyPresent() =>
        Assert.True(VmActionModelAnimation.HasBaseSpriteModelFrames(hasKey: true));

    [Fact]
    public void ResolvePrimaryAnimId_PrefersActorSlot() =>
        Assert.Equal(2, VmActionModelAnimation.ResolvePrimaryAnimId(actorHasAnimationIds: true, actorAnimId0: 2, classAnimId0: 0));

    [Fact]
    public void TryPickModelIndex_UsesAnimId() =>
        Assert.True(VmActionModelAnimation.TryPickModelIndex(1, modelCount: 4, fallbackModelId: 0, out var idx) && idx == 1);

    [Fact]
    public void TryPickModelIndex_FallsBackToModelId() =>
        Assert.True(VmActionModelAnimation.TryPickModelIndex(animId: 9, modelCount: 4, fallbackModelId: 2, out var idx) && idx == 2);

    [Fact]
    public void TryPickModelIndex_UsesBaseSpriteModelId() =>
        Assert.True(VmActionModelAnimation.TryPickBaseSpriteModelIndex(baseSpriteModelId: 3, modelCount: 5, out var idx) && idx == 3);

    [Fact]
    public void ResolveFallbackModelId_PrefersModelDataSlot() =>
        Assert.Equal(4, VmActionModelAnimation.ResolveFallbackModelId(modelDataModelId0: 4, baseSpriteModelId0: 1));

    [Fact]
    public void FindFModelMissingFrames_WhenNoBaseSpriteKey() =>
        Assert.True(VmActionModelAnimation.FindFModelMissingFrames(hasBaseSpriteKey: false));
}
