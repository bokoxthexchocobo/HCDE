namespace HCDE.Playsim.Tests;

public class CoopArmorMetadataTests
{
    [Fact]
    public void ArmorLossRetainsBasicArmorMetadataAndRemovesSpares()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(100, 50, 20, 10, "CustomArmor");
        inventory.TryKeepArmorPickup(80, 50);
        inventory.AbsorbCount = 7;
        inventory.FilterCoopRespawn(false, false, false, true, false, false);
        Assert.Equal(0, inventory.Armor);
        Assert.Equal(33, inventory.ArmorSavePercent);
        Assert.Equal(100, inventory.ArmorMaximum);
        Assert.Equal(100, inventory.ArmorActualSaveAmount);
        Assert.Equal("CustomArmor", inventory.ArmorType);
        Assert.Equal(20, inventory.MaxAbsorb);
        Assert.Equal(10, inventory.MaxFullAbsorb);
        Assert.Equal(7, inventory.AbsorbCount);
        Assert.Empty(inventory.SpareArmor);
    }

    [Fact]
    public void LoseEverythingStillResetsAllArmorMetadata()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(100, 50, 20, 10, "CustomArmor");
        inventory.FilterCoopRespawn(true, false, false, true, false, false);
        Assert.Equal(0, inventory.Armor);
        Assert.Equal(1, inventory.ArmorMaximum);
        Assert.Equal(0, inventory.ArmorActualSaveAmount);
        Assert.Equal("None", inventory.ArmorType);
        Assert.Equal(0, inventory.MaxAbsorb);
    }
}
