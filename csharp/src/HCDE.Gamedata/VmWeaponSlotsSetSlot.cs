namespace HCDE.Gamedata;

/// <summary><c>setslot</c> / <c>addslot</c> helpers from <c>a_weapons.cpp</c> (516–665).</summary>
public static class VmWeaponSlotsSetSlot
{
    public static bool IsValidSlotNumber(int slot) =>
        slot >= 0 && slot < VmWeaponSlotsRuntime.NumWeaponSlots;

    public static bool ShouldClearBeforeAssign(bool setSlotStrict) => setSlotStrict;

    public static int CountWeaponsInArgs(int argc) => Math.Max(0, argc - 2);
}
