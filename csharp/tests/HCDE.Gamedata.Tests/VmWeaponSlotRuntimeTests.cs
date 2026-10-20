namespace HCDE.Gamedata.Tests;

public class VmWeaponSlotRuntimeTests
{
    [Fact]
    public void AddWeaponList_AppendsWeapons()
    {
        var slot = new List<VmWeaponSlotEntry>();
        var types = new HashSet<string> { "Pistol", "Shotgun" };
        VmWeaponSlotRuntime.AddWeaponList(slot, "Pistol Shotgun", false, types);
        Assert.Equal(2, slot.Count);
    }

    [Fact]
    public void SetInitialPositions_SingleWeaponCentered()
    {
        var slot = new List<VmWeaponSlotEntry> { new() { TypeName = "Pistol" } };
        VmWeaponSlotRuntime.SetInitialPositions(slot);
        Assert.Equal(0x8000, slot[0].Position);
    }
}
