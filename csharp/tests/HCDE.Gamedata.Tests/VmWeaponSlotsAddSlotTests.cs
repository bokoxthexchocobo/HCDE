namespace HCDE.Gamedata.Tests;

public class VmWeaponSlotsAddSlotTests
{
    [Fact]
    public void TryParseAddSlotArgs_ParsesSlot()
    {
        Assert.True(VmWeaponSlotsAddSlot.TryParseAddSlotArgs(3, "2", "Shotgun", out var slot));
        Assert.Equal(2, slot);
    }
}
