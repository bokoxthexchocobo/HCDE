namespace HCDE.Playsim.Tests;

public class VmActorPrecalcBonesTests
{
    [Fact]
    public void BoneOverrideApplyCount_TakesMinimum()
    {
        Assert.Equal(3, VmActorPrecalcBones.BoneOverrideApplyCount(5, 3));
        Assert.Equal(2, VmActorPrecalcBones.BoneOverrideApplyCount(2, 8));
    }

    [Fact]
    public void RangeBoneApplyEnd_ClampsStartPlusLength()
    {
        Assert.Equal(9, VmActorPrecalcBones.RangeBoneApplyEnd(10, 10, 5, 4));
        Assert.Equal(5, VmActorPrecalcBones.RangeBoneApplyEnd(10, 5, 0, 100));
    }

    [Fact]
    public void ShouldRecalcBonesAfterApply_SkipsUiPath()
    {
        Assert.True(VmActorPrecalcBones.ShouldRecalcBonesAfterApply(1, uiPath: false));
        Assert.False(VmActorPrecalcBones.ShouldRecalcBonesAfterApply(1, uiPath: true));
    }

    [Fact]
    public void CountMaskedBoneApplications_CountsTrueEntries()
    {
        ReadOnlySpan<bool> mask = [true, false, true];
        Assert.Equal(2, VmActorPrecalcBones.CountMaskedBoneApplications(mask, 3));
    }

    [Fact]
    public void GrowBoneOverridesToModelJointCount_DetectsResize()
    {
        Assert.True(VmActorPrecalcBones.GrowBoneOverridesToModelJointCount(0, 10, out var size));
        Assert.Equal(10, size);
    }

    [Fact]
    public void OverwriteRangeWithinBounds_ValidatesRange()
    {
        Assert.True(VmActorPrecalcBones.OverwriteRangeWithinBounds(2, 3, 10, 10));
        Assert.False(VmActorPrecalcBones.OverwriteRangeWithinBounds(-1, 3, 10, 10));
    }

    [Fact]
    public void UiBoneApplyTic_UsesTimeFrac()
    {
        Assert.Equal(3.25, VmActorPrecalcBones.UiBoneApplyTic(3, 0.25));
    }
}
