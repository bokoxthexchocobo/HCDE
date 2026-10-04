namespace HCDE.Playsim.Tests;

public class StoredArmorPickupActivationTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(100)]
    public void ActiveArmorPreventsAutomaticUseOfStorablePickup(int worn)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(worn, 25, 5, 2, "WornArmor");
        Assert.True(player.Inventory.TryKeepArmorPickup(80, 50, 20, 10, "StoredArmor"));
        Assert.Equal(worn, player.Inventory.Armor);
        Assert.Equal("WornArmor", player.Inventory.ArmorType);
        Assert.Equal(25, player.Inventory.ArmorSavePercent);
        Assert.Equal(5, player.Inventory.MaxAbsorb);
        Assert.Equal(2, player.Inventory.MaxFullAbsorb);
        Assert.Equal(1, AcsPlayerInventory.Count(player, "StoredArmor", false));
    }

    [Fact]
    public void StrongerStoredArmorCanBeUsedExplicitlyAfterPickup()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(1, 25, armorType: "WornArmor");
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        Assert.Equal(1, AcsPlayerInventory.Use(player, "StoredArmor"));
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal("StoredArmor", player.Inventory.ArmorType);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Fact]
    public void StrongerReserveActivatesWhenWornArmorDepletes()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(1, 100, armorType: "WornArmor");
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor");
        ActorDamage.Apply(player, 1);
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal("StoredArmor", player.Inventory.ArmorType);
        Assert.Equal(100, player.Health);
        Assert.Empty(player.Inventory.SpareArmor);
    }

    [Fact]
    public void EmptyArmorAutoActivatesStorablePickup()
    {
        var player = new PlayerPawn();
        Assert.True(player.Inventory.TryKeepArmorPickup(80, 50, armorType: "StoredArmor"));
        Assert.Equal(80, player.Inventory.Armor);
        Assert.Equal("StoredArmor", player.Inventory.ArmorType);
        Assert.Empty(player.Inventory.SpareArmor);
    }
}
