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

    private static VmPMobjCalcBones.CalcBonesContext Sample(bool model, bool picnum = false) =>
        new(model, false, VmPMobjCalcBones.ModeldataGetBoneInfo, picnum, true, 0, 10, true, false, true, 0, 1);
}
