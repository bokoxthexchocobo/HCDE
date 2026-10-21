namespace HCDE.Playsim.Tests;

public class VmActorBoneTransformNativesTests
{
    [Fact]
    public void SetModelBoneRotationNative_SkipsWithoutModel()
    {
        Assert.False(VmActorBoneTransformNatives.SetModelBoneRotationNative(
            false, 0, 1, 0, 0, 1, 0, 0, 1, out _, out var calc));
        Assert.False(calc);
    }

    [Fact]
    public void SetBoneRotationNative_UsesDefaultModelIndex() =>
        Assert.Equal(0, VmActorBoneTransformNatives.DefaultModelIndex);

    [Fact]
    public void SetModelBoneTranslationNative_CalcBonesWhenModelOk()
    {
        Assert.True(VmActorBoneTransformNatives.SetModelBoneTranslationNative(
            true, 0, 1, 0.5, 1, 1, 0, 0, out _, out var calc));
        Assert.True(calc);
    }

    [Fact]
    public void SetModelBoneScalingNative_SampleTime()
    {
        Assert.True(VmActorBoneTransformNatives.SetModelBoneScalingNative(
            true, 4, 1, 0, 0, 1, 2, 3, out var t, out _));
        Assert.Equal(5, t);
    }

    [Fact]
    public void ClearBoneOffsetsNative_RequiresModelData() =>
        Assert.False(VmActorBoneTransformNatives.ClearBoneOffsetsNative(false));

    [Fact]
    public void FinalizeGetBoneOffsetNumret_CapsAtThree() =>
        Assert.Equal(3, VmActorBoneTransformNatives.FinalizeGetBoneOffsetNumret(5));

    [Fact]
    public void GetBoneOffsetNative_DefaultsWhenNoModel()
    {
        Assert.True(VmActorBoneTransformNatives.GetBoneOffsetNative(false, 2, out var n, out var defaults));
        Assert.True(defaults);
        Assert.Equal(2, n);
    }

    [Fact]
    public void GetNamedBoneOffsetNative_MatchesIndexedPath()
    {
        Assert.True(VmActorBoneTransformNatives.GetNamedBoneOffsetNative(
            true, 3, out var n, out var defaults));
        Assert.False(defaults);
        Assert.Equal(3, n);
    }

    [Fact]
    public void GetBoneOffsetAssignsScaling_WhenNumretAboveTwo() =>
        Assert.True(VmActorBoneTransformNatives.GetBoneOffsetAssignsScaling(3));
}
