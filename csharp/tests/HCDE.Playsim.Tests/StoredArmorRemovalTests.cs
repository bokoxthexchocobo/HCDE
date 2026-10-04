namespace HCDE.Playsim.Tests;

public class StoredArmorRemovalTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(int.MaxValue, 0)]
    public void TakingStoredClassRemovesRequestedCopiesOnly(int amount, int remaining)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 25, armorType: "WornArmor");
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        player.Inventory.TryKeepArmorPickup(70, 50, armorType: "OtherArmor");
        player.Inventory.TryKeepArmorPickup(60, 50, armorType: "StoredArmor");
        AcsPlayerInventory.Take(player, "storedarmor", amount);
        Assert.Equal(remaining, AcsPlayerInventory.Count(player, "StoredArmor", false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "OtherArmor", false));
        Assert.Equal(100, player.Inventory.Armor);
        Assert.Equal("WornArmor", player.Inventory.ArmorType);
        if (remaining > 0)
            Assert.Equal(60, player.Inventory.SpareArmor.Last().SaveAmount);
    }

    [Fact]
    public void MissingAndNonpositiveRemovalLeaveStoredSuitIntact()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        AcsPlayerInventory.Take(player, "MissingArmor", 1);
        AcsPlayerInventory.Take(player, "StoredArmor", 0);
        AcsPlayerInventory.Take(player, "StoredArmor", -1);
        Assert.Single(player.Inventory.SpareArmor);
        player.Inventory.Armor = 0;
        Assert.Equal(1, AcsPlayerInventory.Use(player, "StoredArmor"));
    }
}
