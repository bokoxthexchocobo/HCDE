namespace HCDE.Playsim.Tests;

public class ArmorActualSaveAmountTests
{
    [Theory]
    [InlineData(PickupCatalog.GreenArmor, 100)]
    [InlineData(PickupCatalog.MegaArmor, 200)]
    public void QueryReportsSuitAmountInsteadOfAbsorptionCounter(int pickup, int expected)
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, pickup);
        player.Inventory.Armor = 1;
        player.Inventory.AbsorbCount = 17;
        Assert.Equal(expected, AcsPlayerInventory.ArmorInfo(player, 5, new AcsGlobalStrings()));
    }

    [Fact]
    public void ActiveBonusPreservesActualSuitAmountWhileRaisingMaximum()
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, PickupCatalog.GreenArmor);
        PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus);
        Assert.Equal(200, player.Inventory.ArmorMaximum);
        Assert.Equal(100, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void BonusReinitializesActualAmountOnlyAfterDepletion()
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, PickupCatalog.GreenArmor);
        player.Inventory.Armor = 0;
        AcsPlayerInventory.Give(player, "ArmorBonus", 1);
        Assert.Equal(200, player.Inventory.ArmorActualSaveAmount);
    }

    [Fact]
    public void StoredSuitPromotionAndResetUpdateActualAmount()
    {
        var inventory = new PlayerInventory();
        inventory.TryKeepArmorPickup(80, 25);
        inventory.TryKeepArmorPickup(60, 50);
        inventory.Armor = 0;
        inventory.PromoteSpareArmor();
        Assert.Equal(60, inventory.ArmorActualSaveAmount);
        inventory.ResetToPistolStart();
        Assert.Equal(0, inventory.ArmorActualSaveAmount);
    }
}
