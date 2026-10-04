namespace HCDE.Playsim.Tests;

public class ArmorGrantClassNameTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void MegaArmorShorthandDoesNotGrantArmor(int amount)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "MegaArmor", amount);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.Equal("None", player.Inventory.ArmorType);
        Assert.Equal(0, player.Inventory.ArmorActualSaveAmount);
        Assert.Equal(1, player.Inventory.ArmorMaximum);
    }

    [Theory]
    [InlineData("MegaArmor")]
    [InlineData("megaarmor")]
    public void ShorthandStringGrantPreservesExistingArmorMetadata(string type)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 25, 20, 10, "CustomArmor");
        AcsPlayerInventory.Give(player, [type], 0, 1);
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal(25, player.Inventory.ArmorSavePercent);
        Assert.Equal(20, player.Inventory.MaxAbsorb);
        Assert.Equal(10, player.Inventory.MaxFullAbsorb);
        Assert.Equal("CustomArmor", player.Inventory.ArmorType);
        Assert.Equal(100, player.Inventory.ArmorActualSaveAmount);
    }
}
