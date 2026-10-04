namespace HCDE.Playsim.Tests;

public class InternalArmorUseTests
{
    [Theory]
    [InlineData("Armor", false)]
    [InlineData("BasicArmor", false)]
    [InlineData("Armor", true)]
    [InlineData("BasicArmor", true)]
    public void InternalArmorUseDoesNotGrantOrModifyArmor(string name, bool equipped)
    {
        var player = new PlayerPawn();
        if (equipped) player.Inventory.TryKeepArmorPickup(80, 50, 20, 10, "CustomArmor");
        var inventory = player.Inventory;
        var before = (inventory.Armor, inventory.ArmorMaximum, inventory.ArmorType,
            inventory.ArmorSavePercent, inventory.MaxAbsorb, inventory.MaxFullAbsorb);
        Assert.Equal(0, AcsPlayerInventory.Use(player, name));
        Assert.Equal(before, (inventory.Armor, inventory.ArmorMaximum, inventory.ArmorType,
            inventory.ArmorSavePercent, inventory.MaxAbsorb, inventory.MaxFullAbsorb));
    }
}
