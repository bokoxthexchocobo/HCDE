namespace HCDE.Playsim.Tests;

public class InventoryDropQuantityTests
{
    [Theory]
    [InlineData("Clip")]
    [InlineData("Shell")]
    [InlineData("RocketAmmo")]
    [InlineData("Cell")]
    public void AmmoDropRemovesOneUnitAndRetainsCapacity(string type)
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 0;
        AcsPlayerInventory.Give(player, type, 5);
        var maximum = AcsPlayerInventory.Count(player, type, true);
        Assert.True(AcsPlayerInventory.Drop(player, type));
        Assert.Equal(4, AcsPlayerInventory.Count(player, type, false));
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, type, true));
    }

    [Fact]
    public void ReserveDropRemovesOnlyFirstMatchingCopy()
    {
        var player = new PlayerPawn();
        player.Inventory.Armor = 100;
        player.Inventory.TryKeepArmorPickup(80, 50, armorType: "Reserve");
        player.Inventory.TryKeepArmorPickup(60, 50, armorType: "Reserve");
        Assert.True(AcsPlayerInventory.Drop(player, "Reserve"));
        Assert.Equal(60, player.Inventory.SpareArmor.Single().SaveAmount);
        Assert.Equal(100, player.Inventory.Armor);
    }

    [Theory]
    [InlineData("Armor")]
    [InlineData("BasicArmor")]
    [InlineData("Fist")]
    public void ClassesWithoutPickupStateRemainOwned(string type)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "BlueArmor", 1);
        Assert.False(AcsPlayerInventory.Drop(player, type));
        Assert.Equal(200, player.Inventory.Armor);
        Assert.True(player.Inventory.Owns(WeaponKind.Fist));
    }

    [Fact]
    public void LastAmmoUnitDropsButDepletedPoolDoesNot()
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 1;
        Assert.True(AcsPlayerInventory.Drop(player, "Clip"));
        Assert.Equal(0, player.Inventory.Bullets);
        Assert.False(AcsPlayerInventory.Drop(player, "Clip"));
        Assert.Equal(200, player.Inventory.MaxBullets);
    }
}
