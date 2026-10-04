namespace HCDE.Playsim.Tests;

public class ArmorSourceIdentityTests
{
    [Theory]
    [InlineData(PickupCatalog.GreenArmor, "GreenArmor")]
    [InlineData(PickupCatalog.MegaArmor, "BlueArmor")]
    [InlineData(PickupCatalog.ArmorBonus, "ArmorBonus")]
    public void ArmorInfoReportsPickupSource(int pickup, string expected)
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, pickup);
        var globals = new AcsGlobalStrings();
        Assert.Equal(expected, globals.Get(AcsPlayerInventory.ArmorInfo(player, 0, globals)));
    }

    [Fact]
    public void ActiveBonusRetainsSuitIdentityAndDepletedBonusReplacesIt()
    {
        var player = new PlayerPawn();
        PickupCatalog.TryGive(player, PickupCatalog.GreenArmor);
        PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus);
        Assert.Equal("GreenArmor", player.Inventory.ArmorType);
        player.Inventory.Armor = 0;
        PickupCatalog.TryGive(player, PickupCatalog.ArmorBonus);
        Assert.Equal("ArmorBonus", player.Inventory.ArmorType);
        player.Inventory.ResetToPistolStart();
        Assert.Equal("None", player.Inventory.ArmorType);
    }
}
