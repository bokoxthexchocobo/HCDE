namespace HCDE.Playsim.Tests;

public class ArmorMaximumQueryTests
{
    [Theory]
    [InlineData(PickupCatalog.GreenArmor, 100)]
    [InlineData(PickupCatalog.MegaArmor, 200)]
    public void SuitCapacitySurvivesDepletion(int pickup, int expected)
    {
        var player = new PlayerPawn();
        Assert.True(PickupCatalog.TryGive(player, pickup));
        AcsPlayerInventory.Take(player, "Armor", expected);
        Assert.Equal(0, AcsPlayerInventory.Count(player, "Armor", false));
        Assert.Equal(expected, AcsPlayerInventory.Count(player, "Armor", true));
    }

    [Fact]
    public void BonusRaisesMaximumAndNewSuitReplacesIt()
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
        PickupCatalog.TryGive(player, PickupCatalog.GreenArmor);
        Assert.Equal(100, player.Inventory.ArmorMaximum);
        PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
    }

    [Fact]
    public void StoredSuitPromotionUpdatesMaximum()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(80, 25);
        inventory.TryKeepArmorPickup(60, 50);
        inventory.Armor = 0;
        inventory.PromoteSpareArmor();
        Assert.Equal(60, inventory.ArmorMaximum);
        inventory.ResetToPistolStart();
        Assert.Equal(1, inventory.ArmorMaximum);
    }
}
