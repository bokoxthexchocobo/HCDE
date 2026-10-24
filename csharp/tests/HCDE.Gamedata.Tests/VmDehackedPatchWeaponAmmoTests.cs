namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchWeaponAmmoTests
{
    [Fact]
    public void ApplyAmmoType_DoublesGiveAmount()
    {
        var target = new VmDehackedWeaponPatchTarget();
        VmDehackedPatchWeaponAmmo.ApplyAmmoType(target, "Clip", 10);
        Assert.Equal(20, target.AmmoGive);
        Assert.Equal(1, target.AmmoUse);
    }

    [Fact]
    public void FinalizeAmmo_ClearsUseWhenNoType()
    {
        var target = new VmDehackedWeaponPatchTarget { AmmoUse = 5 };
        VmDehackedPatchWeaponAmmo.FinalizeAmmo(target);
        Assert.Equal(0, target.AmmoUse);
    }
}
