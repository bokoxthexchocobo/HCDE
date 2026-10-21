namespace HCDE.Playsim.Tests;

public class VmActionSetBonesOverridesTests
{
    [Fact]
    public void MinBoneLoopCount_TakesMinimum() =>
        Assert.Equal(3, VmActionSetBonesOverrides.MinBoneLoopCount(frameCount: 3, overrideCount: 10));

    [Fact]
    public void RangeLoopEnd_CapsByLength() =>
        Assert.Equal(8, VmActionSetBonesOverrides.RangeLoopEnd(20, 20, start: 5, length: 3));

    [Fact]
    public void ShouldCalcBonesAfterSet_WhenAnyBone() =>
        Assert.True(VmActionSetBonesOverrides.ShouldCalcBonesAfterSet(n: 1));

    [Fact]
    public void MaskSelectsBone_WhenTrue() =>
        Assert.True(VmActionSetBonesOverrides.MaskSelectsBone(maskValue: true));

    [Fact]
    public void RenderStyleNotFound_ReturnsNegOne() =>
        Assert.Equal(-1, VmActionSetBonesOverrides.RenderStyleNotFound());

    [Fact]
    public void SetBonesUiTic_UsesFrac() =>
        Assert.Equal(3.25, VmActionSetBonesOverrides.SetBonesUiTic(3.0, 0.25));

    [Fact]
    public void OverwriteModeUsesMode() =>
        Assert.True(VmActionSetBonesOverrides.OverwriteModeUsesMode(mode: 0));

    [Fact]
    public void RangeLoopInBounds() =>
        Assert.True(VmActionSetBonesOverrides.RangeLoopInBounds(i: 6, n: 10, start: 5));

    [Fact]
    public void ForceRecalculate_WhenSelf() =>
        Assert.True(VmActionSetBonesOverrides.ForceRecalculateInvokesCalcBones(selfNonNull: true));

    [Fact]
    public void SetModelFlagNativeForwards() =>
        Assert.True(VmActionSetBonesOverrides.SetModelFlagNativeForwards(selfNonNull: true));
}
