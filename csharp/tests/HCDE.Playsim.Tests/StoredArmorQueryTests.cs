namespace HCDE.Playsim.Tests;

public class StoredArmorQueryTests
{
    [Fact]
    public void CurrentCountTracksStoredSuitConsumptionWithoutCountingWornArmor()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 25, armorType: "WornArmor");
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        Assert.Equal(0, AcsPlayerInventory.Count(player, "WornArmor", false));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "storedarmor", false));
        Assert.Equal(0, AcsPlayerInventory.Use(player, "StoredArmor"));
        Assert.Equal(1, AcsPlayerInventory.Count(player, "StoredArmor", false));
        player.Inventory.Armor = 50;
        Assert.Equal(1, AcsPlayerInventory.Use(player, "StoredArmor"));
        Assert.Equal(0, AcsPlayerInventory.Count(player, "StoredArmor", false));
    }

    [Fact]
    public void RepeatedStoredClassCountsEachRetainedCopy()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        Assert.Equal(2, AcsPlayerInventory.Count(player, "StoredArmor", false));
        player.Inventory.Armor = 0;
        player.Inventory.PromoteSpareArmor();
        Assert.Equal(1, AcsPlayerInventory.Count(player, "StoredArmor", false));
        player.Inventory.ResetToPistolStart();
        Assert.Equal(0, AcsPlayerInventory.Count(player, "StoredArmor", false));
    }
}
