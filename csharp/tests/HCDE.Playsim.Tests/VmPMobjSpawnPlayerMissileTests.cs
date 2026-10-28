namespace HCDE.Playsim.Tests;

public class VmPMobjSpawnPlayerMissileTests
{
    [Fact]
    public void RejectNullSpawn_WhenTypeMissing()
    {
        Assert.True(VmPMobjSpawnPlayerMissile.RejectNullSpawn(false, true));
    }

    [Fact]
    public void SanitizeAimFlags_ClearsIgnoreBit()
    {
        Assert.Equal(0, VmPMobjSpawnPlayerMissile.SanitizeAimFlags(VmPMobjSpawnPlayerMissile.AlfIgnoreNoAutoaim));
    }

    [Fact]
    public void UseWeaponAimLock_WhenNoAutoAimOnWeapon()
    {
        Assert.True(VmPMobjSpawnPlayerMissile.UseWeaponAimLock(
            hasPlayer: true,
            hasReadyWeapon: true,
            weaponFlags: VmPMobjSpawnPlayerMissile.WifNoautoaim,
            noAutoAim: false,
            aimFlags: 0));
    }

    [Fact]
    public void ResolvePlayerMissileAim_WeaponLockKeepsPitch()
    {
        var result = VmPMobjSpawnPlayerMissile.ResolvePlayerMissileAim(
            weaponAimLock: true,
            inputAngle: 90,
            sourcePitch: 15,
            noFreeAim: false,
            freelookAllowed: true,
            _ => false,
            hasLineTargetAfterLoop: false);
        Assert.Equal(90, result.Yaw);
        Assert.Equal(15, result.Pitch);
    }

    [Fact]
    public void CustomSpawnZ_DistinguishesFloorConstants()
    {
        Assert.False(VmPMobjSpawnPlayerMissile.CustomSpawnZ(VmPMobjSpawnPlayerMissile.OnFloorZ));
        Assert.True(VmPMobjSpawnPlayerMissile.CustomSpawnZ(12));
    }

    [Fact]
    public void ClampSpawnZToFloor_RaisesZ()
    {
        Assert.Equal(4, VmPMobjSpawnPlayerMissile.ClampSpawnZToFloor(2, 4));
    }

    [Fact]
    public void DefaultSpawnAngle_Sentinel()
    {
        Assert.True(VmPMobjSpawnPlayerMissile.DefaultSpawnAngle(1e37));
    }
}
