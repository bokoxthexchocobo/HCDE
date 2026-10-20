namespace HCDE.Gamedata.Tests;

public class VmWeaponSlotsSetSlotTests
{
    [Fact]
    public void IsValidSlotNumber_RejectsTen()
    {
        Assert.False(VmWeaponSlotsSetSlot.IsValidSlotNumber(10));
    }
}
