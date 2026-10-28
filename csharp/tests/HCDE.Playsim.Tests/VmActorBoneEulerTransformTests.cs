namespace HCDE.Playsim.Tests;

public class VmActorBoneEulerTransformTests
{
    [Fact]
    public void GetBoneEulerAnglesNative_ReturnsEulerWhenModelOk() =>
        Assert.Equal((1, 2, 3), VmActorBoneEulerTransform.GetBoneEulerAnglesNative(true, false, 1, 2, 3));

    [Fact]
    public void GetBoneEulerAnglesNative_DefaultsWhenNoModel() =>
        Assert.Equal((0, 0, 0), VmActorBoneEulerTransform.GetBoneEulerAnglesNative(false, false, 1, 2, 3));

    [Fact]
    public void TransformByBoneNative_ReturnsNumret() =>
        Assert.Equal(2, VmActorBoneEulerTransform.TransformByBoneNative(
            true, false, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, false, 0, 0, 0, 0, 0, 0, 0, 0, 0));

    [Fact]
    public void GetBoneMatrixRawNative_RequiresOutMatrix()
    {
        VmActorBoneEulerTransform.GetBoneMatrixRawNative(false, true, false, 0, out var fill);
        Assert.False(fill);
    }
}
