namespace HCDE.Playsim.Tests;

public class VmPMobjZMovementTests
{
    [Fact]
    public void ZMovement_MissileHitsFloor_Explodes()
    {
        var exploded = false;
        var mo = new VmPMobjZMovement.ZActor
        {
            Z = 0,
            FloorZ = 0,
            VelZ = -1,
            Flags = VmPMobjZMovement.MfMissile,
            Height = 8,
        };
        var st = new VmPMobjZMovement.ZMovementState
        {
            Actor = mo,
            OldFloorZ = 0,
            ExplodeMissile = (_, _, _, _) => exploded = true,
        };
        VmPMobjZMovement.ZMovement(st);
        Assert.True(exploded);
    }

    [Fact]
    public void ZMovement_SmoothStepUp_AdjustsView()
    {
        double? delta = null;
        var mo = new VmPMobjZMovement.ZActor
        {
            HasPlayer = true,
            Z = 0,
            FloorZ = 32,
            VelZ = 0,
        };
        var st = new VmPMobjZMovement.ZMovementState
        {
            Actor = mo,
            OldFloorZ = 0,
            SmoothStepUpView = (_, d, _) => delta = d,
            FallAndSink = (_, _, _) => { },
        };
        VmPMobjZMovement.ZMovement(st);
        Assert.Equal(32, delta);
    }

    [Fact]
    public void MissileFloorExplode_TrueForPlainMissile()
    {
        Assert.True(VmPMobjZMovement.MissileFloorExplode(
            VmPMobjZMovement.MfMissile, 0, 0, 0));
    }
}
