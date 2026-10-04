namespace HCDE.Playsim.Tests;

public class AcsDirectArmorGrantTests
{
    [Theory]
    [InlineData("Armor", false)]
    [InlineData("Armor", true)]
    [InlineData("BasicArmor", false)]
    [InlineData("BasicArmor", true)]
    public void BaseArmorGrantLeavesDefaultInventoryUnchanged(string name, bool equipped)
    {
        var player = new PlayerPawn();
        if (equipped) player.Inventory.TryKeepArmorPickup(80, 50, 20, 10, "CustomArmor");
        var inventory = player.Inventory;
        var before = (inventory.Armor, inventory.ArmorMaximum, inventory.ArmorActualSaveAmount,
            inventory.ArmorSavePercent, inventory.ArmorType, inventory.MaxAbsorb, inventory.MaxFullAbsorb);
        AcsPlayerInventory.Give(player, name, 200);
        Assert.Equal(before, (inventory.Armor, inventory.ArmorMaximum, inventory.ArmorActualSaveAmount,
            inventory.ArmorSavePercent, inventory.ArmorType, inventory.MaxAbsorb, inventory.MaxFullAbsorb));
    }

    [Theory]
    [InlineData("Armor")]
    [InlineData("BasicArmor")]
    public void BaseArmorGrantDoesNotReplenishDepletedSuit(string name)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "BlueArmor", 1);
        AcsPlayerInventory.Take(player, "Armor", 200);
        AcsPlayerInventory.Give(player, name, int.MaxValue);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
        Assert.Equal("BlueArmor", player.Inventory.ArmorType);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
    }
}
