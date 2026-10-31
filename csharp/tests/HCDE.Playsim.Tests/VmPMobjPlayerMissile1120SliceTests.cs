namespace HCDE.Playsim.Tests;

public class VmPMobjPlayerMissile1120SliceTests
{
    [Fact]
    public void SubMissileVerticalSpeed_NegatesSine()
    {
        Assert.Equal(0, VmPMobjPlayerMissile1120Slice.SubMissileVerticalSpeed(10, 0), 5);
        Assert.True(VmPMobjPlayerMissile1120Slice.SubMissileVerticalSpeed(10, Math.PI / 2) < 0);
    }

    [Fact]
    public void AimRange_UsesSixteenTilesWhenUnset()
    {
        Assert.Equal(1024, VmPMobjPlayerMissile1120Slice.AimRange(0));
        Assert.Equal(128, VmPMobjPlayerMissile1120Slice.AimRange(2));
    }

    [Fact]
    public void RunSpawnPlayerMissile_KeepsPlayerPitchWhenWeaponDisablesAutoAim()
    {
        var ctx = new VmPMobjPlayerMissile1120Slice.PlayerMissileContext
        {
            HasPlayer = true,
            HasReadyWeapon = true,
            WeaponFlags = VmPMobjPlayerMissile1120Slice.WifNoAutoAim,
            Angle = 15,
            PlayerPitch = -4,
            Z = 10,
            ZIsFloorSentinel = true
        };
        Assert.True(VmPMobjPlayerMissile1120Slice.RunSpawnPlayerMissile(ctx));
        Assert.Equal(15, ctx.ChosenAngle);
        Assert.Equal(-4, ctx.ChosenPitch);
        Assert.True(ctx.ClearedLineTarget);
    }

    [Fact]
    public void RunSpawnPlayerMissile_ClampsSpawnBelowFloor()
    {
        var ctx = new VmPMobjPlayerMissile1120Slice.PlayerMissileContext
        {
            HasPlayer = true,
            NoAutoAim = true,
            Z = 1,
            Center = 0,
            FloorClip = 0,
            AttackOffset = -4,
            FloorZ = 8
        };
        Assert.True(VmPMobjPlayerMissile1120Slice.RunSpawnPlayerMissile(ctx));
        Assert.Equal(8, ctx.Z);
        Assert.True(ctx.ClampedToFloor);
    }
}
