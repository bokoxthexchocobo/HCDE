namespace HCDE.Gamedata.Tests;

public class VmDehackedFinishWeaponAmmoTests
{
    [Fact]
    public void ResolveAmmoUseFromAttack_UsesBfgCellsWhenNegative()
    {
        Assert.Equal(40, VmDehackedFinishWeaponAmmo.ResolveAmmoUseFromAttack(-1, 40));
    }

    [Fact]
    public void ClearIntryMove_ClearsMf6()
    {
        Assert.Equal(0u, VmDehackedFinishWeaponAmmo.ClearIntryMove(VmDehackedFinishDehPatch.Mf6IntryMove));
    }

    [Fact]
    public void TryProcessWeaponAmmo_ConsumeAmmoStateClearsFlag()
    {
        uint flags = VmDehackedFinishWeaponAmmo.StfConsumeAmmo;
        Assert.True(VmDehackedFinishWeaponAmmo.TryProcessWeaponAmmo(
            0, true, ref flags, 0, 0, out var use, out _));
        Assert.Equal(0u, flags);
        Assert.Equal(-1, use);
    }
}
