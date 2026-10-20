namespace HCDE.Gamedata;

/// <summary><c>FinishDehPatch</c> weapon ammo post-process from <c>d_dehacked.cpp</c> (3902–3964).</summary>
public static class VmDehackedFinishWeaponAmmo
{
    public const uint StfConsumeAmmo = 1u;

    public static bool ShouldClearIntryMove(uint flags6) =>
        (flags6 & VmDehackedFinishDehPatch.Mf6IntryMove) != 0;

    public static uint ClearIntryMove(uint flags6) =>
        flags6 & ~VmDehackedFinishDehPatch.Mf6IntryMove;

    public static int ResolveAmmoUseFromAttack(int attackAmmoCount, int bfgCells) =>
        attackAmmoCount < 0 ? bfgCells : attackAmmoCount;

    public static int FinalizeAmmoUse1(bool handled, int resolvedUse) => handled ? resolvedUse : 0;

    public static bool TryProcessWeaponAmmo(
        uint weaponFlags6,
        bool consumeAmmoState,
        ref uint stateFlags,
        int attackAmmoCount,
        int bfgCells,
        out int ammoUse1,
        out bool setDehAmmoBool)
    {
        ammoUse1 = 0;
        setDehAmmoBool = false;
        if (ShouldClearIntryMove(weaponFlags6))
            return true;
        setDehAmmoBool = true;
        if (consumeAmmoState)
        {
            stateFlags &= ~StfConsumeAmmo;
            ammoUse1 = -1;
            return true;
        }

        var use = ResolveAmmoUseFromAttack(attackAmmoCount, bfgCells);
        ammoUse1 = FinalizeAmmoUse1(attackAmmoCount >= 0 || attackAmmoCount < 0, use);
        return true;
    }
}
