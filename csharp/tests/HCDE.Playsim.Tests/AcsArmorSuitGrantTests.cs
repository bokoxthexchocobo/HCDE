namespace HCDE.Playsim.Tests;

public class AcsArmorSuitGrantTests
{
    [Theory]
    [InlineData("GreenArmor", 1, 100, 33)]
    [InlineData("GreenArmor", 3, 300, 33)]
    [InlineData("BlueArmor", 1, 200, 50)]
    [InlineData("BlueArmor", 2, 400, 50)]
    [InlineData("BlueArmor", 3, 600, 50)]
    public void SuitGrantUsesScaledSaveAmount(string name, int count, int armor, int percent)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, name, count);
        Assert.Equal(armor, player.Inventory.Armor);
        Assert.Equal(armor, player.Inventory.ArmorMaximum);
        Assert.Equal(percent, player.Inventory.ArmorSavePercent);
    }

    [Fact]
    public void WeakerSuitGrantLeavesExistingSuitIntact()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "BlueArmor", 1);
        AcsPlayerInventory.Give(player, "GreenArmor", 1);
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
    }
}
