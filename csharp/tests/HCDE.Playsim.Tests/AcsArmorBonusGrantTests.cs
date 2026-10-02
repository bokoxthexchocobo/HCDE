namespace HCDE.Playsim.Tests;

public class AcsArmorBonusGrantTests
{
    [Theory]
    [InlineData(7, 7)]
    [InlineData(250, 200)]
    [InlineData(int.MaxValue, 200)]
    public void BonusCountAddsArmorUpToCapWithoutOverflow(int count, int expected)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "armorbonus", count);
        Assert.Equal(expected, player.Inventory.Armor);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
        Assert.Equal(33, player.Inventory.ArmorSavePercent);
    }

    [Fact]
    public void BonusRetainsActiveSuitProtectionAndAbsorptionCaps()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(150, 50, 20, 10);
        AcsPlayerInventory.Give(player, "ArmorBonus", 100);
        Assert.Equal(200, player.Inventory.Armor);
        Assert.Equal(50, player.Inventory.ArmorSavePercent);
        Assert.Equal(20, player.Inventory.MaxAbsorb);
        Assert.Equal(10, player.Inventory.MaxFullAbsorb);
    }

    [Fact]
    public void FullArmorRejectsBonusWithoutChangingCapacity()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(300, 50);
        AcsPlayerInventory.Give(player, "ArmorBonus", 100);
        Assert.Equal(300, player.Inventory.Armor);
        Assert.Equal(300, player.Inventory.ArmorMaximum);
    }

    [Fact]
    public void CatalogBonusHonorsExplicitAmount()
    {
        var player = new PlayerPawn();
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus, pickupAmount: 12));
        Assert.Equal(12, player.Inventory.Armor);
    }
}
