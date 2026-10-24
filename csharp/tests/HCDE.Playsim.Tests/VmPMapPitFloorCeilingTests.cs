namespace HCDE.Playsim.Tests;

public class VmPMapPitFloorCeilingTests
{
    [Fact]
    public void FloorRaise_BridgeNoFit() =>
        Assert.True(VmPMapPitFloorCeiling.FloorRaise_BridgeBlocks(
            VmPMapPitFloorCeiling.Mf4Actlikebridge));

    [Fact]
    public void FloorRaise_CrunchOnCeiling() =>
        Assert.True(VmPMapPitFloorCeiling.FloorRaise_CrunchDefault(
            new VmPMapPitFloorCeiling.FloorRaisePushOutcome(
                VmPMapPitFloorCeiling.PushCeiling)));

    [Fact]
    public void CeilingLower_TargetUsesFloor() =>
        Assert.Equal(4, VmPMapPitFloorCeiling.CeilingLower_TargetZ(10, 8, 4));

    [Fact]
    public void CeilingLower_RevertWhenOnFloor() =>
        Assert.True(VmPMapPitFloorCeiling.CeilingLower_RevertToFloor(
            new VmPMapPitFloorCeiling.CeilingLowerOutcome(
                VmPMapPitFloorCeiling.PushBlocked, true)));

    [Fact]
    public void CeilingRaise_DoomCompat() =>
        Assert.True(VmPMapPitFloorCeiling.CeilingRaise_DoomMove(0, 8, 16, 20, 4, 0));

    [Fact]
    public void CeilingRaise_PassobjStack() =>
        Assert.True(VmPMapPitFloorCeiling.CeilingRaise_PassobjRelocate(
            true, false, 10, 32, 8));

    [Fact]
    public void FloorRaise_RidingBranch() =>
        Assert.True(VmPMapPitFloorCeiling.FloorRaise_TakeRidingBranch(
            new VmPMapPitFloorCeiling.FloorRaiseBranchContext(4, 8, 0, 0)));
}
