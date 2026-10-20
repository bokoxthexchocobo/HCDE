namespace HCDE.Playsim.Tests;

public class VmPMobjBoneWorldTickTests
{
    [Fact]
    public void BonePositionGate_BlocksPicnum() =>
        Assert.False(VmPMobjBoneWorldTick.EvaluateBonePositionGate(true, 1, 0, 0, 1, 1, true));

    [Fact]
    public void BoneWorld_SwizzlesAxes() =>
        Assert.Equal((1, 3, 2), VmPMobjBoneWorldTick.SwizzlePosition(1, 2, 3));

    [Fact]
    public void ObjectToWorldMatrix_GateRequiresBoneFlag() =>
        Assert.True(VmPMobjBoneWorldTick.GetObjectToWorldMatrixGate(true, 1, false));

    [Fact]
    public void CheckNoDelay_ClearsFlagWhenEligible() =>
        Assert.True(VmPMobjBoneWorldTick.CheckNoDelayEligible(1, 0));

    [Fact]
    public void SectorTransition_BuildsHitFloor() =>
        Assert.True((VmPMobjBoneWorldTick.BuildEnterSectorActions(10, 10, 20, 30, false, 0)
            & VmPMobjBoneWorldTick.SecspacHitfloor) != 0);

    [Fact]
    public void WaterDepth_UnderwaterSectorFillsHeight() =>
        Assert.Equal(32, VmPMobjBoneWorldTick.EvaluateWaterDepth(
            new VmPMobjBoneWorldTick.WaterDepthContext(false, VmPMobjBoneWorldTick.SecmfUnderwater, 32, 0, 32, false, false, 0, 0, 0)).Depth);

    [Fact]
    public void Skip3dFloor_MissingOrSolid() =>
        Assert.True(VmPMobjBoneWorldTick.Skip3dFloor(VmPMobjBoneWorldTick.FfSolid));
}
