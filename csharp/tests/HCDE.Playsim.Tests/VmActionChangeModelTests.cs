namespace HCDE.Playsim.Tests;

public class VmActionChangeModelTests
{
    [Fact]
    public void MaskIqmFlags_AndsMask() =>
        Assert.Equal(0xFF & VmActionChangeModel.ModeldataIqmflagsMask, VmActionChangeModel.MaskIqmFlags(0xFF));

    [Fact]
    public void NormalizeModelIndex_NegativeBecomesZero() =>
        Assert.Equal(0, VmActionChangeModel.NormalizeModelIndex(-1));

    [Fact]
    public void EnsureTrailingSlash_AppendsSlash() =>
        Assert.Equal("models/", VmActionChangeModel.EnsureTrailingSlash("models"));

    [Fact]
    public void QueryModelWhenVisible_HideModel() =>
        Assert.Equal(-2, VmActionChangeModel.QueryModelWhenVisible(VmActionChangeModel.CmdlHidemodel, modelNameSet: true));

    [Fact]
    public void SelectInvokerAsMobj_FromInventoryUsesSelf() =>
        Assert.True(VmActionChangeModel.SelectInvokerAsMobj(fromPsprite: false, fromInventory: true, flags: 0, useSelf: true));

    [Fact]
    public void ShouldPushSurfaceSkin_FlagAndIndex() =>
        Assert.True(VmActionChangeModel.ShouldPushSurfaceSkin(VmActionChangeModel.CmdlUsesurfaceskin, skinIndex: 0));

    [Fact]
    public void ShouldResetCurAnimOnAnimationChange_WhenIdDiffers() =>
        Assert.True(VmActionChangeModel.ShouldResetCurAnimOnAnimationChange(
            curAnimFlags: 0, modelAnimNone: 1, animationIndex: 0, animationId0: 1, queryAnimation: 2));

    [Fact]
    public void InvalidModelDefWhenNamed_MissingClass() =>
        Assert.True(VmActionChangeModel.InvalidModelDefWhenNamed(modelDefNamed: true, classFound: false));

    [Fact]
    public void AppendNewModelSlot_WhenCountsMatch() =>
        Assert.True(VmActionChangeModel.AppendNewModelSlot(modelCount: 2, modelIndex: 2));

    [Fact]
    public void ShouldCalcBonesAfterChange_ModelDefOnly() =>
        Assert.True(VmActionChangeModel.ShouldCalcBonesAfterChange(animationNamed: false, modelDefSet: true));

    [Fact]
    public void ShouldSetOverrideFlags_WhenFlagNonZero() =>
        Assert.True(VmActionChangeModel.ShouldSetOverrideFlags(4));

    [Fact]
    public void ShouldResetModelOverrides_WhenResetModel() =>
        Assert.True(VmActionChangeModel.ShouldResetModelOverrides(1));

    [Fact]
    public void UseWeaponToPlayer_WhenCmdlFlag() =>
        Assert.True(VmActionChangeModel.UseWeaponToPlayer(VmActionChangeModel.CmdlWeapontoplayer));

    [Fact]
    public void ShouldGrowSurfaceSkins_WhenIndexPastEnd() =>
        Assert.True(VmActionChangeModel.ShouldGrowSurfaceSkins(existingCount: 1, skinIndex: 3));

    [Fact]
    public void ShouldAssignSkinAtEnd_WhenCountsMatch() =>
        Assert.True(VmActionChangeModel.ShouldAssignSkinAtEnd(skinIdCount: 2, skinIndex: 2));
}
