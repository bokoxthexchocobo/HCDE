namespace HCDE.Net.Core;

/// <summary><c>Net_DoCommand</c> weapon slot cases (<c>d_net.cpp</c> 9099–9133).</summary>
public static class VmDNetDoCommandWeaponSlots
{
    public static bool ShouldClearWeaponSlotNative(int slot, int numWeaponSlots) =>
        slot >= 0 && slot < numWeaponSlots;

    public static bool ShouldAddWeaponToSlotNative(int slot, object? weaponClass) =>
        slot >= 0 && weaponClass is not null;

    public static bool UseConsolePlayerForSlotAddNative(int player, int consolePlayer) => player == consolePlayer;

    public static bool ShouldReadSlotPlayerNumberNative(int cmd, int demSetSlotPnum) => cmd == demSetSlotPnum;
}
