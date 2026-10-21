namespace HCDE.Playsim.Tests;

public class VmActorBoneInfoGettersTests
{
    [Fact]
    public void GetRootBonesNative_RequiresOutPointer()
    {
        Assert.False(VmActorBoneInfoGetters.GetRootBonesNative(false, true, out var fill));
        Assert.False(fill);
    }

    [Fact]
    public void GetRootBonesNative_FillsWhenModelOk()
    {
        Assert.True(VmActorBoneInfoGetters.GetRootBonesNative(true, true, out var fill));
        Assert.True(fill);
    }

    [Fact]
    public void GetBoneNameNative_ReturnsModelIndexWhenOk() =>
        Assert.Equal(42, VmActorBoneInfoGetters.GetBoneNameNative(true, 7, 42));

    [Fact]
    public void GetBoneIndexNative_ReturnsResolvedIndex() =>
        Assert.Equal(3, VmActorBoneInfoGetters.GetBoneIndexNative(true, 3));

    [Fact]
    public void GetBoneParentNative_UsesJointParent() =>
        Assert.Equal(5, VmActorBoneInfoGetters.GetBoneParentNative(true, 5));

    [Fact]
    public void GetBoneChildrenNative_RequiresValidBone()
    {
        Assert.True(VmActorBoneInfoGetters.GetBoneChildrenNative(true, true, 0, out var fill));
        Assert.True(fill);
    }

    [Fact]
    public void FinalizeBoneBaseTrsNumret_CapsAtThree() =>
        Assert.Equal(3, VmActorBoneInfoGetters.FinalizeBoneBaseTrsNumret(4));

    [Fact]
    public void GetBoneBaseTrsNative_DefaultsWithoutModel()
    {
        Assert.True(VmActorBoneInfoGetters.GetBoneBaseTrsNative(
            false, 0, 1, false, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0,
            out var n, out _, out _, out _, out _, out _, out _, out var rw, out _, out _, out _));
        Assert.Equal(1, n);
        Assert.Equal(1, rw);
    }

    [Fact]
    public void GetBoneBaseTrsAssignsScaling_WhenNumretAboveTwo() =>
        Assert.True(VmActorBoneInfoGetters.GetBoneBaseTrsAssignsScaling(3));

    [Fact]
    public void GetBoneCountNative_ReturnsJointCount() =>
        Assert.Equal(12, VmActorBoneInfoGetters.GetBoneCountNative(true, 12));
}
