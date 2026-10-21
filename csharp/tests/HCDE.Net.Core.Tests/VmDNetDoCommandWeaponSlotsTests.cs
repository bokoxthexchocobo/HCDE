namespace HCDE.Net.Core.Tests;

public class VmDNetDoCommandWeaponSlotsTests
{
    [Fact]
    public void ShouldClearWeaponSlotNative_WithinRange() =>
        Assert.True(VmDNetDoCommandWeaponSlots.ShouldClearWeaponSlotNative(0, 10));

    [Fact]
    public void ShouldAddWeaponToSlotNative_RequiresClass() =>
        Assert.True(VmDNetDoCommandWeaponSlots.ShouldAddWeaponToSlotNative(0, new object()));
}
