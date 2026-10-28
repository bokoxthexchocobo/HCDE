namespace HCDE.Gamedata.Tests;

public class VmDehackedPatchAmmoTests
{
    [Fact]
    public void ApplyBackpackAmounts_DoublesMax()
    {
        var ammo = new VmDehackedAmmoPatchTarget { MaxAmount = 50, PerClip = 5 };
        VmDehackedPatchAmmo.ApplyBackpackAmounts(ammo);
        Assert.Equal(100, ammo.BackpackMaxAmount);
        Assert.Equal(5, ammo.BackpackAmount);
    }

    [Fact]
    public void ScaleGiveAmount_Proportional()
    {
        Assert.Equal(20, VmDehackedPatchAmmo.ScaleGiveAmount(10, 4, 2));
    }
}
