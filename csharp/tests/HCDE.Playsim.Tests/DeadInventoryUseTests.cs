namespace HCDE.Playsim.Tests;

public class DeadInventoryUseTests
{
    [Theory]
    [InlineData("Health", 0)]
    [InlineData("Medikit", -10)]
    [InlineData("ArmorBonus", 0)]
    [InlineData("BlueArmor", -10)]
    [InlineData("Backpack", 0)]
    [InlineData("Shotgun", -10)]
    public void DeadPlayerCannotUseInventory(string name, int health)
    {
        var player = new PlayerPawn { Health = health };
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        player.Inventory.Pending = WeaponKind.Chaingun;
        Assert.Equal(0, AcsPlayerInventory.Use(player, name));
        Assert.Equal(health, player.Health);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(WeaponKind.Chaingun, player.Inventory.Pending);
    }

    [Fact]
    public void SetWeaponRetainsItsSeparateSelectionRulesForDeadPlayer()
    {
        var player = new PlayerPawn { Health = 0 };
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 8;
        Assert.Equal(1, AcsPlayerInventory.SetWeapon(player, "Shotgun"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }
}
