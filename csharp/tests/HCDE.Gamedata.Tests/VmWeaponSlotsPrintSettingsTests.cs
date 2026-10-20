namespace HCDE.Gamedata.Tests;

public class VmWeaponSlotsPrintSettingsTests
{
    [Fact]
    public void EnumerateNonEmptySlots_SkipsEmpty()
    {
        var slots = new List<List<VmWeaponSlotEntry>> { new(), new() { new VmWeaponSlotEntry { TypeName = "Pistol", Position = 0 } } };
        var found = VmWeaponSlotsPrintSettings.EnumerateNonEmptySlots(slots).ToList();
        Assert.Single(found);
    }
}
