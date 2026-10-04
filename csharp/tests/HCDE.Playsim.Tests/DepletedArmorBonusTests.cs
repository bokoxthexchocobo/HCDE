namespace HCDE.Playsim.Tests;

public class DepletedArmorBonusTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void BonusResetsDepletedSuitProtectionAndLimits(bool acs)
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 50, 20, 10);
        AcsPlayerInventory.Take(player, "Armor", 100);
        player.Inventory.ArmorSavePercent = 50;
        if (acs) AcsPlayerInventory.Give(player, "ArmorBonus", 7);
        else Assert.True(PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus, pickupAmount: 7));
        Assert.Equal(7, player.Inventory.Armor);
        Assert.Equal(33, player.Inventory.ArmorSavePercent);
        Assert.Equal(0, player.Inventory.MaxAbsorb);
        Assert.Equal(0, player.Inventory.MaxFullAbsorb);
    }

    [Fact]
    public void ActiveZeroProtectionSuitIsNotReinitializedByBonus()
    {
        var player = new PlayerPawn();
        player.Inventory.TryKeepArmorPickup(100, 0, 20, 10);
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus));
        Assert.Equal(101, player.Inventory.Armor);
        Assert.Equal(0, player.Inventory.ArmorSavePercent);
        Assert.Equal(20, player.Inventory.MaxAbsorb);
        Assert.Equal(10, player.Inventory.MaxFullAbsorb);
    }
}
