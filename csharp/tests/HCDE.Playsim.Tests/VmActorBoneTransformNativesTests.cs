namespace HCDE.Playsim.Tests;

public class VmActorBoneTransformNativesTests
{
    [Fact]
    public void SetModelBoneRotationNative_SkipsWithoutModel()
    {
        VmActorBoneTransformNatives.SetModelBoneRotationNative(
            false, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1.0, out var sample, out var calc);
        Assert.Equal(0, sample);
        Assert.False(calc);
    }

    [Fact]
    public void SetBoneRotationNative_UsesDefaultModelIndex()
    {
        VmActorBoneTransformNatives.SetBoneRotationNative(
            true, 4, 2, 0, 0, 0, 1, 0, 0, 1.0, out var sample, out var calc);
        Assert.Equal(5, sample);
        Assert.True(calc);
    }

    [Fact]
    public void SetBoneRotationActionWrapper_UsesTicFracOne()
    {
        VmActorBoneTransformNatives.SetBoneRotationActionWrapper(
            2, 0, 0, 0, 0, 1, 0, 0, true, out var sample, out var calc);
        Assert.Equal(3, sample);
        Assert.True(calc);
    }

    [Fact]
    public void SetModelBoneTranslationNative_CalcBonesWhenModelOk()
    {
        VmActorBoneTransformNatives.SetModelBoneTranslationNative(
            true, 0, 0, 0, 1, 2, 3, 0, 0.5, 1.0, out var sample, out var calc);
        Assert.Equal(1, sample);
        Assert.True(calc);
    }

    [Fact]
    public void SetModelBoneScalingNative_SampleTime()
    {
        VmActorBoneTransformNatives.SetModelBoneScalingNative(
            true, 4, 0, 0, 1, 2, 3, 0, 0, 1.0, out var t, out var calc);
        Assert.Equal(5, t);
        Assert.True(calc);
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
        VmActorBoneTransformNatives.GetBoneOffsetNative(
            false, 0, 2, 0, false, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0,
            out var n, out _, out _, out _, out _, out _, out _, out var rw, out _, out _, out _);
        Assert.Equal(2, n);
        Assert.Equal(1, rw);
    }

    [Fact]
    public void GetBoneOffsetAssignsScaling_WhenNumretAboveTwo() =>
        Assert.Equal(3, VmActorBoneTransformNatives.FinalizeGetBoneOffsetNumret(4));

    [Fact]
    public void GetNamedBoneOffsetNative_MatchesIndexedPath()
    {
        VmActorBoneTransformNatives.GetNamedBoneOffsetNative(
            true, 1, 3, 0, true, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0,
            out var n, out var tx, out _, out _, out _, out _, out _, out _, out _, out _, out _);
        Assert.Equal(3, n);
        Assert.Equal(1, tx);
    }
}
