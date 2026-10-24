namespace HCDE.Gamedata;

/// <summary><c>addslot</c> command validation from <c>a_weapons.cpp</c> (816–945).</summary>
public static class VmWeaponSlotsAddSlot
{
    public static bool TryParseAddSlotArgs(int argc, string slotToken, string weaponToken, out int slot)
    {
        slot = -1;
        if (argc != 3) return false;
        if (!int.TryParse(slotToken, out slot)) return false;
        return IsValidSlot(slot) && weaponToken.Length > 0;
    }

    public static bool IsValidSlot(int slot) => VmWeaponSlotsSetSlot.IsValidSlotNumber(slot);
}
