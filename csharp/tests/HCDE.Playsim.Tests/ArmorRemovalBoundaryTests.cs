namespace HCDE.Playsim.Tests;

public class ArmorRemovalBoundaryTests
{
    [Theory]
    [InlineData(int.MinValue, 1, 0)]
    [InlineData(-1, int.MaxValue, 0)]
    [InlineData(0, 1, 0)]
    [InlineData(100, 99, 1)]
    [InlineData(100, 100, 0)]
    [InlineData(int.MaxValue, 1, int.MaxValue - 1)]
    [InlineData(int.MaxValue, int.MaxValue, 0)]
    public void PositiveRemovalFollowsNativeDepletionWithoutOverflow(int armor, int take, int expected)
    {
        foreach (var name in new[] { "Armor", "BasicArmor" })
        {
            var player = new PlayerPawn();
            player.Inventory.TryKeepArmorPickup(80, 50, 20, 10, "WornArmor");
            player.Inventory.Armor = armor;
            AcsPlayerInventory.Take(player, name, take);
            Assert.Equal(expected, player.Inventory.Armor);
            Assert.Equal("WornArmor", player.Inventory.ArmorType);
            Assert.Equal(80, player.Inventory.ArmorMaximum);
            Assert.Equal(80, player.Inventory.ArmorActualSaveAmount);
            Assert.Equal(50, player.Inventory.ArmorSavePercent);
            Assert.Equal(20, player.Inventory.MaxAbsorb);
            Assert.Equal(10, player.Inventory.MaxFullAbsorb);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NonpositiveScriptRequestLeavesEvenNegativeArmorUntouched(int take)
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = int.MinValue;
        AcsPlayerInventory.Take(player, "Armor", take);
        Assert.Equal(int.MinValue, player.Inventory.Armor);
    }
}
