namespace HCDE.Playsim.Tests;

public class AcsArmorUseParityTests
{
    [Fact]
    public void DepletedBonusUseResetsProtectionLimitsAndActualAmount()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 50, 20, 10);
        player.Inventory.Armor = 0;
        AcsPlayerInventory.Give(player, "ArmorBonus", 1);
        Assert.Equal(33, player.Inventory.ArmorSavePercent);
        Assert.Equal(0, player.Inventory.MaxAbsorb);
        Assert.Equal(0, player.Inventory.MaxFullAbsorb);
        Assert.Equal(200, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal("ArmorBonus", player.Inventory.ArmorType);
    }

    [Theory]
    [InlineData("BlueArmor")]
    [InlineData("bluearmor")]
    public void SuitUseReplacesProtectionAndClearsOldLimits(string name)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 33, 20, 10);
        AcsPlayerInventory.Give(player, name, 1);
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal(0, player.Inventory.MaxAbsorb);
        Assert.Equal(0, player.Inventory.MaxFullAbsorb);
        Assert.Equal("BlueArmor", player.Inventory.ArmorType);
    }

    [Fact]
    public void ActiveZeroProtectionBonusUseRetainsExistingMetadata()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 0, 20, 10, "CustomArmor");
        AcsPlayerInventory.Give(player, "ArmorBonus", 1);
        Assert.Equal(0, player.Inventory.ArmorSavePercent);
        Assert.Equal(20, player.Inventory.MaxAbsorb);
        Assert.Equal("CustomArmor", player.Inventory.ArmorType);
        Assert.Equal(100, player.Inventory.ArmorActualSaveAmount);
    }
}
