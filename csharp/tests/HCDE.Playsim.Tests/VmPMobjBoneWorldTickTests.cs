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
    public void BoneWorld_NormalizesForward() =>
        Assert.Equal((1, 0, 0), VmPMobjBoneWorldTick.NormalizeVector(2, 0, 0));

    [Fact]
    public void ObjectToWorldMatrix_GateRequiresBoneFlag() =>
        Assert.True(VmPMobjBoneWorldTick.GetObjectToWorldMatrixGate(true, 1, false));

    [Fact]
    public void CopyMatrixFlat_Copies16() =>
        Assert.Equal(16, VmPMobjBoneWorldTick.CopyMatrixFlat(new double[16], new double[16]));

    [Fact]
    public void Unmorph_StopsWhenFlagSet() =>
        Assert.True(VmPMobjBoneWorldTick.TickStopsOnUnmorph(
            new VmPMobjBoneWorldTick.UnmorphTickContext(true, false, VmPMobjBoneWorldTick.MfUnmorphed, false)));

    [Fact]
    public void Freeze_DecrementsTicks()
    {
        var t = 2;
        Assert.True(VmPMobjBoneWorldTick.FreezeTickDecrement(ref t));
        Assert.Equal(1, t);
    }

    [Fact]
    public void DecoupledAnimations_RequestsCalcBones() =>
        Assert.True(VmPMobjBoneWorldTick.ShouldCalcBonesOnTick(VmPMobjBoneWorldTick.Mf9Decoupledanimations));

    [Fact]
    public void NointeractionFrozen_BossCubeSpecial() =>
        Assert.True(VmPMobjBoneWorldTick.NointeractionFrozenBossCube(
            new VmPMobjBoneWorldTick.NointeractionFrozenContext(true, VmPMobjBoneWorldTick.Mf6Bosscube, 0, false, 0)));

    [Fact]
    public void HexenScrollTables_IndexSafely() =>
        Assert.Equal(-25, VmPMobjBoneWorldTick.HexenScrollSpeed(0));

    [Fact]
    public void TickPrologue_DestroyOnNullState() =>
        Assert.True(VmPMobjBoneWorldTick.EvaluateTickPrologue(
            new VmPMobjBoneWorldTick.UnmorphTickContext(false, false, 0, false),
            ref int.MaxValue,
            0,
            true,
            0,
            false,
            false,
            new VmPMobjBoneWorldTick.NointeractionMoveContext(true, 0, false)).DestroyActor);
}
