namespace HCDE.Playsim.Tests;

public class VmActorBoneInfoGettersTests
{
    [Fact]
    public void GetRootBonesNative_SkipsNullOut()
    {
        VmActorBoneInfoGetters.GetRootBonesNative(false, true);
    }

    [Fact]
    public void GetBoneNameNative_ReturnsJointNameIndex() =>
        Assert.Equal(42, VmActorBoneInfoGetters.GetBoneNameNative(42));

    [Fact]
    public void GetBoneIndexNative_ReturnsResolvedIndex() =>
        Assert.Equal(3, VmActorBoneInfoGetters.GetBoneIndexNative(3));

    [Fact]
    public void GetBoneParentNative_ReturnsParent() =>
        Assert.Equal(5, VmActorBoneInfoGetters.GetBoneParentNative(5));

    [Fact]
    public void GetBoneBaseTrsNative_CapsNumretAtThree()
    {
        var n = VmActorBoneInfoGetters.GetBoneBaseTrsNative(
            true, 0, 5, false, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0);
        Assert.Equal(3, n);
    }

    [Fact]
    public void GetBoneCountNative_ReturnsJointCount() =>
        Assert.Equal(12, VmActorBoneInfoGetters.GetBoneCountNative(12));
}
