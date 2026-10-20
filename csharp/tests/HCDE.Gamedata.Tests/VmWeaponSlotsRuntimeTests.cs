namespace HCDE.Gamedata.Tests;

public class VmWeaponSlotsRuntimeTests
{
    [Fact]
    public void TryLocateWeapon_FindsAcrossSlots()
    {
        var slots = new List<List<VmWeaponSlotEntry>>
        {
            new(),
            new() { new() { TypeName = "Shotgun" } }
        };
        Assert.True(VmWeaponSlotsRuntime.TryLocateWeapon(slots, "Shotgun", out var slot, out var index));
        Assert.Equal(1, slot);
        Assert.Equal(0, index);
    }

    [Fact]
    public void ApplyDefaultSlotWeapons_SkipsWhenSlotsPopulated()
    {
        var slots = new List<List<VmWeaponSlotEntry>> { new() { new() { TypeName = "Pistol" } } };
        VmWeaponSlotsRuntime.ApplyDefaultSlotWeapons(slots, [[ "Shotgun" ]], new HashSet<string> { "Shotgun" });
        Assert.Single(slots[0]);
    }
}
