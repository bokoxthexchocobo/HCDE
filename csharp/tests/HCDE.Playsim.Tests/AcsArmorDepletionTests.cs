namespace HCDE.Playsim.Tests;

public class AcsArmorDepletionTests
{
    [Theory]
    [InlineData("Armor", 100)]
    [InlineData("BasicArmor", 150)]
    [InlineData("Armor", int.MaxValue)]
    public void TakingAllArmorRetainsMetadataAndDoesNotPromoteSpare(string name, int amount)
    {
        var player = new PlayerPawn();
        var inventory = player.Inventory;
        inventory.TryKeepArmorPickup(100, 50, 20, 10, "CustomArmor");
        inventory.TryKeepArmorPickup(80, 50);
        inventory.AbsorbCount = 7;
        AcsPlayerInventory.Take(player, name, amount);
        Assert.Equal(0, inventory.Armor);
        Assert.Equal(50, inventory.ArmorSavePercent);
        Assert.Equal(100, inventory.ArmorMaximum);
        Assert.Equal(100, inventory.ArmorActualSaveAmount);
        Assert.Equal("CustomArmor", inventory.ArmorType);
        Assert.Equal(20, inventory.MaxAbsorb);
        Assert.Equal(10, inventory.MaxFullAbsorb);
        Assert.Equal(7, inventory.AbsorbCount);
        Assert.Single(inventory.SpareArmor);
    }

    [Fact]
    public void BonusAfterAcsDepletionReinitializesProtection()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 50, 20, 10);
        AcsPlayerInventory.Take(player, "Armor", 100);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        AcsPlayerInventory.Give(player, "ArmorBonus", 1);
        Assert.Equal(33, player.Inventory.ArmorSavePercent);
        Assert.Equal(0, player.Inventory.MaxAbsorb);
        Assert.Equal("ArmorBonus", player.Inventory.ArmorType);
    }
}
