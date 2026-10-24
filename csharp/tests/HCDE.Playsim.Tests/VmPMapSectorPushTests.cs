namespace HCDE.Playsim.Tests;

public class VmPMapSectorPushTests
{
    [Fact]
    public void PushUp_CeilingBlock() =>
        Assert.True(VmPMapSectorPush.PushUp_BlocksOnCeiling(20, 10));

    [Fact]
    public void PushUp_ThruActors() =>
        Assert.True(VmPMapSectorPush.PushUp_SkipThruActors(VmPMapSectorPush.Mf2Thruactors));

    [Fact]
    public void Push_SkipReflective() =>
        Assert.True(VmPMapSectorPush.SkipReflectiveMissile(
            VmPMapSectorPush.MfMissile,
            VmPMapSectorPush.Mf2Reflective,
            VmPMapSectorPush.Mf7Thureflect));

    [Fact]
    public void Push_CannotPushBridge() =>
        Assert.True(VmPMapSectorPush.CannotPushIntersect(
            VmPMapSectorPush.Mf2Passmobj, 0, VmPMapSectorPush.Mf4Actlikebridge, 1, 10));

    [Fact]
    public void PushDown_FloorBlock() =>
        Assert.True(VmPMapSectorPush.PushDown_BlocksOnFloor(0, 0));

    [Fact]
    public void FloorDrop_MovesWithSector() =>
        Assert.True(VmPMapSectorPush.FloorDrop_GravityMove(
            0, VmPMapSectorPush.Mf5Movewithsector, 10, 8, 0, 4));

    [Fact]
    public void FloorRaise_BridgeNoFit() =>
        Assert.True(VmPMapSectorPush.FloorRaise_MarkNoFitBridge(
            VmPMapSectorPush.Mf4Actlikebridge));

    [Fact]
    public void PushUp_RestoreOnIntersectBlock() =>
        Assert.True(VmPMapSectorPush.PushUp_RestoreOldZ(
            new VmPMapSectorPush.PushUpDispatchContext(
                VmPMapSectorPush.PushIntersectBlocked, false)));

    [Fact]
    public void CeilingLower_CrunchWhenBlocked() =>
        Assert.True(VmPMapSectorPush.CeilingLower_CrunchOnPushFail(
            new VmPMapSectorPush.CeilingLowerPushContext(
                VmPMapSectorPush.PushIntersectBlocked, true)));

    [Fact]
    public void CeilingRaise_DoomCompatBand() =>
        Assert.True(VmPMapSectorPush.CeilingRaise_DoomCompatMove(0, 8, 16, 20, 4, 0));

    [Fact]
    public void SkipPushIntersect_Species() =>
        Assert.True(VmPMapSectorPush.SkipPushIntersect(
            new VmPMapSectorPush.PushIntersectContext(
                VmPMapSectorPush.Mf6Thruspecies, true, 0, 0, 0)));
}
