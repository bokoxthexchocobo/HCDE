namespace HCDE.Playsim.Tests;

public class VmPMobjHitFloorSpawnTests
{
    [Fact]
    public void HitFloor_TouchyCrush() =>
        Assert.True(VmPMobjHitFloorSpawn.TouchyCrushOnImpact(
            VmPMobjHitFloorSpawn.Mf6Touchy | VmPMobjHitFloorSpawn.Mf6Armed, true, -10));

    [Fact]
    public void HitFloor_3dFloorTopSplash() =>
        Assert.True(VmPMobjHitFloorSpawn.TryHitWaterVia3dFloor(
            new VmPMobjHitFloorSpawn.HitFloorSectorWalkContext(
                true, 12, 0, VmPMobjHitFloorSpawn.FfExists | VmPMobjHitFloorSpawn.FfSolid, 12)));

    [Fact]
    public void NetRegister_SkipsNull() =>
        Assert.False(VmPMobjHitFloorSpawn.RegisterMissileNet(true, false));

    [Fact]
    public void DefaultSpeed_FastMonster() =>
        Assert.Equal(2.5, VmPMobjHitFloorSpawn.PickDefaultSpeed(true, 2.5, 1.0));

    [Fact]
    public void SpawnMissile_RejectNullDest() =>
        Assert.True(VmPMobjHitFloorSpawn.SpawnMissile_NullDest(true));

    [Fact]
    public void SpawnMissile_HuggerClearsZ() =>
        Assert.True(VmPMobjHitFloorSpawn.HuggerClearsVertical(
            VmPMobjHitFloorSpawn.Mf3Floorhugger));

    [Fact]
    public void SpawnMissile_RaiseOverTallTarget() =>
        Assert.True(VmPMobjHitFloorSpawn.RaiseTrajectoryOverTarget(64, 0, 32));

    [Fact]
    public void SpawnMissile_SpectralSetsFriend() =>
        Assert.True(VmPMobjHitFloorSpawn.SpectralFriend(VmPMobjHitFloorSpawn.Mf4Spectral));

    [Fact]
    public void SpawnMissile_RejectInvalidCombo() =>
        Assert.True(VmPMobjHitFloorSpawn.RejectInvalidSpawn(
            new VmPMobjHitFloorSpawn.SpawnMissileValidationContext(false, false, true)));
}
