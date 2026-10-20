namespace HCDE.Gamedata;

/// <summary><c>FWeaponSlots</c> locate/add helpers from <c>a_weapons.cpp</c>.</summary>
public static class VmWeaponSlotsRuntime
{
    public const int NumWeaponSlots = 10;

    public static bool TryLocateWeapon(IReadOnlyList<List<VmWeaponSlotEntry>> slots, string typeName, out int slot, out int index)
    {
        for (var s = 0; s < slots.Count; s++)
        {
            var idx = VmWeaponSlotRuntime.LocateWeapon(slots[s], typeName);
            if (idx >= 0)
            {
                slot = s;
                index = idx;
                return true;
            }
        }

        slot = -1;
        index = -1;
        return false;
    }

    public static int AddDefaultWeapon(List<List<VmWeaponSlotEntry>> slots, int slotIndex, string typeName, HashSet<string> weaponTypes)
    {
        if (TryLocateWeapon(slots, typeName, out _, out _)) return 1;
        if (slotIndex < 0 || slotIndex >= NumWeaponSlots) return 2;
        return VmWeaponSlotRuntime.TryAddWeapon(slots[slotIndex], typeName, weaponTypes) ? 0 : 2;
    }

    public static void SetInitialPositionsAllSlots(IReadOnlyList<List<VmWeaponSlotEntry>> slots)
    {
        foreach (var slot in slots)
            VmWeaponSlotRuntime.SetInitialPositions(slot);
    }

    public static void SortAllSlots(List<List<VmWeaponSlotEntry>> slots)
    {
        foreach (var slot in slots)
            VmWeaponSlotRuntime.StableSortByPosition(slot);
    }

    public static bool AllSlotsEmpty(IReadOnlyList<List<VmWeaponSlotEntry>> slots)
    {
        foreach (var slot in slots)
            if (slot.Count > 0) return false;
        return true;
    }

    public static void AddFromDefaultList(List<VmWeaponSlotEntry> slot, IReadOnlyList<string> typeNames, HashSet<string> weaponTypes)
    {
        foreach (var name in typeNames)
            VmWeaponSlotRuntime.TryAddWeapon(slot, name, weaponTypes);
    }

    public static void ApplyDefaultSlotWeapons(List<List<VmWeaponSlotEntry>> slots, IReadOnlyList<IReadOnlyList<string>> defaults, HashSet<string> weaponTypes)
    {
        if (!AllSlotsEmpty(slots)) return;
        for (var i = 0; i < defaults.Count && i < slots.Count; i++)
            AddFromDefaultList(slots[i], defaults[i], weaponTypes);
    }
}
