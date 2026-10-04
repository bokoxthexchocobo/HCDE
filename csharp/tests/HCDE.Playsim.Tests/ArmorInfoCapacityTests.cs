namespace HCDE.Playsim.Tests;

public class ArmorInfoCapacityTests
{
    [Theory]
    [InlineData(60, 50)]
    [InlineData(300, 33)]
    [InlineData(90, 0)]
    public void SaveAmountUsesStoredCapacityRegardlessOfProtection(int amount, int percent)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(amount, percent, 20, 10);
        player.Inventory.Armor = 1;
        var globals = new AcsGlobalStrings();
        Assert.Equal(amount, AcsPlayerInventory.ArmorInfo(player, 1, globals));
        Assert.Equal(20, AcsPlayerInventory.ArmorInfo(player, 3, globals));
        Assert.Equal(10, AcsPlayerInventory.ArmorInfo(player, 4, globals));
    }

    [Fact]
    public void DepletedArmorInfoIsZeroDespiteRetainedCapacity()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(300, 50, 20, 10);
        player.Inventory.Armor = 0;
        var globals = new AcsGlobalStrings();
        Assert.Equal(300, AcsPlayerInventory.Count(player, "Armor", true));
        foreach (var field in new[] { 1, 2, 3, 4, 5 })
            Assert.Equal(0, AcsPlayerInventory.ArmorInfo(player, field, globals));
    }
}
