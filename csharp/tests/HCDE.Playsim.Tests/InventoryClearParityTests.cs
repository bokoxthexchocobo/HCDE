namespace HCDE.Playsim.Tests;

public class InventoryClearParityTests
{
    [Fact]
    public void ClearRemovesWeaponsKeysStoredArmorAndPowerups()
    {
        var player = new PlayerPawn { DrainStrength = 0.5 };
        player.GivePowerBuddha();
        var inventory = player.Inventory;
        inventory.BlueKey = inventory.RedKey = inventory.YellowKey = true;
        inventory.TryKeepArmorPickup(100, 33, armorType: "Worn");
        inventory.TryKeepArmorPickup(80, 50, armorType: "Stored");
        inventory.Pending = WeaponKind.Fist;
        AcsPlayerInventory.Clear(player);
        Assert.Equal(default(WeaponKind), inventory.Weapons);
        Assert.Equal(default(WeaponKind), inventory.Selected);
        Assert.Null(inventory.Pending);
        Assert.False(inventory.BlueKey || inventory.RedKey || inventory.YellowKey);
        Assert.Empty(inventory.SpareArmor);
        Assert.Equal(0, player.PowerBuddhaTics);
        Assert.Equal(0, player.DrainStrength);
        Assert.Equal("None", AcsPlayerInventory.ReadyWeaponClassName(player));
    }

    [Fact]
    public void ClearDepletesArmorWithoutErasingMetadata()
    {
        var player = new PlayerPawn();
        var inventory = player.Inventory;
        inventory.TryKeepArmorPickup(150, 50, 20, 10, "CustomArmor");
        inventory.AbsorbCount = 7;
        AcsPlayerInventory.Clear(player);
        Assert.Equal(0, inventory.Armor);
        Assert.Equal(150, inventory.ArmorMaximum);
        Assert.Equal(150, inventory.ArmorActualSaveAmount);
        Assert.Equal("CustomArmor", inventory.ArmorType);
        Assert.Equal(50, inventory.ArmorSavePercent);
        Assert.Equal(20, inventory.MaxAbsorb);
        Assert.Equal(10, inventory.MaxFullAbsorb);
        Assert.Equal(7, inventory.AbsorbCount);
    }

    [Fact]
    public void ClearDepletesAmmoAndPreservesCustomCapacities()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Give(player, "Backpack", 1);
        AcsPlayerInventory.SetAmmoCapacity(player, "Clip", 777);
        AcsPlayerInventory.SetAmmoCapacity(player, "Cell", -7);
        AcsPlayerInventory.Clear(player);
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(0, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.Equal(0, player.Inventory.Rockets);
        Assert.Equal(0, player.Inventory.Cells);
        Assert.Equal(777, player.Inventory.MaxBullets);
        Assert.Equal(50, player.Inventory.MaxShells);
        Assert.Equal(50, player.Inventory.MaxRockets);
        Assert.Equal(-7, player.Inventory.MaxCells);
    }
}
