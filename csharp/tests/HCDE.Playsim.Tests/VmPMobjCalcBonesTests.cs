namespace HCDE.Playsim.Tests;

public class VmPMobjCalcBonesTests
{
    [Fact]
    public void CalcBones_SkipsWhenNoModelData() =>
        Assert.False(VmPMobjCalcBones.EvaluateCalcBones(Sample(false)));

    [Fact]
    public void CalcBones_SkipsWhenPicnumOverrides() =>
        Assert.False(VmPMobjCalcBones.EvaluateCalcBones(Sample(true, picnum: true)));

    [Fact]
    public void CalcBones_RunsWhenEligible() =>
        Assert.True(VmPMobjCalcBones.EvaluateCalcBones(Sample(true)));

    [Fact]
    public void BoneQuery_ValidIndices() =>
        Assert.True(VmPMobjCalcBones.EvaluateBoneQuery(
            new VmPMobjCalcBones.BoneQueryContext(true, VmPMobjCalcBones.ModeldataGetBoneInfo, 0, 0, 1, 1, false)));

    [Fact]
    public void BoneQuery_BlocksPicnum() =>
        Assert.False(VmPMobjCalcBones.EvaluateBoneQuery(
            new VmPMobjCalcBones.BoneQueryContext(true, VmPMobjCalcBones.ModeldataGetBoneInfo, 0, 0, 1, 1, true)));

    [Fact]
    public void EulerAngles_GimbalLockUsesUpX() =>
        Assert.Equal(-0.5, VmPMobjCalcBones.EvaluateEulerAngles(
            new VmPMobjCalcBones.EulerAnglesContext(true, 0.5, 0, 0, 0, 0, 0, 0, 0)).Roll);

    [Fact]
    public void EulerAngles_SolvesFromXAxis() =>
        Assert.Equal(0.25, VmPMobjCalcBones.EvaluateEulerAngles(
            new VmPMobjCalcBones.EulerAnglesContext(false, 0, 0, 0, 2, 0, 0, 1, 0.5)).Roll);

    private static VmPMobjCalcBones.CalcBonesContext Sample(bool model, bool picnum = false) =>
        new(model, false, VmPMobjCalcBones.ModeldataGetBoneInfo, picnum, true, 0, 10, true, false, true, 0, 1);
}
