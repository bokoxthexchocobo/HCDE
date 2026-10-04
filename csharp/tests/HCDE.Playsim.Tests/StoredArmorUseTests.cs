namespace HCDE.Playsim.Tests;

public class StoredArmorUseTests
{
    [Theory]
    [InlineData("GreenArmor")]
    [InlineData("BlueArmor")]
    [InlineData("ArmorBonus")]
    public void UnownedArmorUseDoesNotGrantArmor(string name)
    {
        var player = new PlayerPawn();
        Assert.Equal(0, AcsPlayerInventory.Use(player, name));
        Assert.Equal(0, player.Inventory.Armor);
    }

    [Fact]
    public void OwnedStoredSuitIsConsumedOnlyAfterSuccessfulUse()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 33);
        player.Inventory.TryKeepArmorPickup(80, 50, 20, 10, "CustomArmor");
        Assert.Equal(0, AcsPlayerInventory.Use(player, "CustomArmor"));
        Assert.Single(player.Inventory.SpareArmor);
        player.Inventory.Armor = 50;
        Assert.Equal(1, AcsPlayerInventory.Use(player, "customarmor"));
        Assert.Empty(player.Inventory.SpareArmor);
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal(20, player.Inventory.MaxAbsorb);
        Assert.Equal("CustomArmor", player.Inventory.ArmorType);
        Assert.Equal(0, AcsPlayerInventory.Use(player, "CustomArmor"));
    }

    [Fact]
    public void DeadPlayerCannotConsumeStoredSuit()
    {
        var player = new PlayerPawn { Health = 0 };
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "CustomArmor");
        player.Inventory.Armor = 0;
        Assert.Equal(0, AcsPlayerInventory.Use(player, "CustomArmor"));
        Assert.Single(player.Inventory.SpareArmor);
    }
}
