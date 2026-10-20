namespace HCDE.Gamedata;

public sealed class VmWeaponSlotEntry
{
    public string TypeName { get; set; } = "";
    public int Position { get; set; } = -1;
}

/// <summary><c>FWeaponSlot</c> helpers from <c>a_weapons.cpp</c>.</summary>
public static class VmWeaponSlotRuntime
{
    public static bool TryAddWeapon(List<VmWeaponSlotEntry> weapons, string typeName, HashSet<string> weaponTypes)
    {
        if (!weaponTypes.Contains(typeName)) return false;
        if (weapons.Any(w => w.TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase))) return true;
        weapons.Add(new VmWeaponSlotEntry { TypeName = typeName });
        return true;
    }

    public static void AddWeaponList(List<VmWeaponSlotEntry> weapons, string list, bool clear, HashSet<string> weaponTypes)
    {
        if (clear) weapons.Clear();
        foreach (var token in list.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            TryAddWeapon(weapons, token, weaponTypes);
    }

    public static int LocateWeapon(IReadOnlyList<VmWeaponSlotEntry> weapons, string typeName)
    {
        for (var i = 0; i < weapons.Count; i++)
            if (weapons[i].TypeName.Equals(typeName, StringComparison.OrdinalIgnoreCase)) return i;
        return -1;
    }

    public static void SetInitialPositions(List<VmWeaponSlotEntry> weapons)
    {
        if (weapons.Count == 1)
        {
            weapons[0].Position = 0x8000;
            return;
        }

        for (var i = 0; i < weapons.Count; i++)
            weapons[i].Position = i * 0xFF00 / (weapons.Count - 1) + 0x80;
    }

    public static void ClearSlot(List<VmWeaponSlotEntry> weapons) => weapons.Clear();

    public static void ClearAllSlots(List<List<VmWeaponSlotEntry>> slots)
    {
        foreach (var slot in slots)
            slot.Clear();
    }

    public static void StableSortByPosition(List<VmWeaponSlotEntry> weapons)
    {
        for (var i = 1; i < weapons.Count; i++)
        {
            var pos = weapons[i].Position;
            var type = weapons[i].TypeName;
            var j = i - 1;
            while (j >= 0 && weapons[j].Position > pos)
            {
                weapons[j + 1] = weapons[j];
                j--;
            }

            weapons[j + 1].TypeName = type;
            weapons[j + 1].Position = pos;
        }
    }
}
