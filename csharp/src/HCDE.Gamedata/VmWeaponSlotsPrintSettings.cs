namespace HCDE.Gamedata;

/// <summary><c>FWeaponSlots::PrintSettings</c> from <c>a_weapons.cpp</c> (666–815).</summary>
public static class VmWeaponSlotsPrintSettings
{
    public static IEnumerable<(int Slot, IReadOnlyList<string> Weapons)> EnumerateNonEmptySlots(
        IReadOnlyList<List<VmWeaponSlotEntry>> slots)
    {
        for (var i = 1; i <= VmWeaponSlotsRuntime.NumWeaponSlots; i++)
        {
            var slot = i % VmWeaponSlotsRuntime.NumWeaponSlots;
            if (slot >= slots.Count || slots[slot].Count == 0) continue;
            yield return (slot, slots[slot].Select(w => w.TypeName).ToList());
        }
    }
}
