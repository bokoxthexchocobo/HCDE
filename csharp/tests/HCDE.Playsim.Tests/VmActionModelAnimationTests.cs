namespace HCDE.Playsim.Tests;

public class VmActionModelAnimationTests
{
    [Fact]
    public void SkipSameAnimation_WhenNooverrideAndSameFrame() =>
        Assert.True(VmActionModelAnimation.SkipSameAnimation(VmActionModelAnimation.SafNooverride, curAnimNone: false, sameStartFrame: true));

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
    public void TryPickBaseSpriteModelIndex_UsesClassDefault() =>
        Assert.True(VmActionModelAnimation.TryPickBaseSpriteModelIndex(baseSpriteModelId: 3, modelCount: 5, out var idx) && idx == 3);

    [Fact]
    public void TryResolveFindFModel_MissingBaseSpriteKey() =>
        Assert.False(VmActionModelAnimation.TryResolveFindFModelModelIndex(
            hasBaseSpriteFramesKey: false,
            actorHasAnimationIds: true,
            actorAnimId0: 1,
            classAnimId0: 0,
            modelsCount: 4,
            hasModelDataModels: true,
            modelDataModelId0: 2,
            baseSpriteModelId0: 0,
            out _));

    [Fact]
    public void TryResolveFindFModel_UsesActorAnimId() =>
        Assert.True(VmActionModelAnimation.TryResolveFindFModelModelIndex(
            hasBaseSpriteFramesKey: true,
            actorHasAnimationIds: true,
            actorAnimId0: 2,
            classAnimId0: 0,
            modelsCount: 4,
            hasModelDataModels: false,
            modelDataModelId0: -1,
            baseSpriteModelId0: 1,
            out var idx) && idx == 2);

    [Fact]
    public void TryResolveFindFModel_UsesModelDataSlotWhenAnimInvalid() =>
        Assert.True(VmActionModelAnimation.TryResolveFindFModelModelIndex(
            hasBaseSpriteFramesKey: true,
            actorHasAnimationIds: false,
            actorAnimId0: -1,
            classAnimId0: -1,
            modelsCount: 4,
            hasModelDataModels: true,
            modelDataModelId0: 2,
            baseSpriteModelId0: 1,
            out var idx) && idx == 2);
}
