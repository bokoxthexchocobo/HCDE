namespace HCDE.Playsim.Tests;

public class AcsWeaponGrantTests
{
    [Theory]
    [InlineData("Pistol", AmmoKind.Bullets, 20)]
    [InlineData("Chaingun", AmmoKind.Bullets, 20)]
    [InlineData("Shotgun", AmmoKind.Shells, 8)]
    [InlineData("SuperShotgun", AmmoKind.Shells, 8)]
    [InlineData("RocketLauncher", AmmoKind.Rockets, 2)]
    [InlineData("PlasmaRifle", AmmoKind.Cells, 40)]
    [InlineData("BFG9000", AmmoKind.Cells, 40)]
    public void WeaponCountDoesNotReplaceAmmoGive(string name, AmmoKind ammo, int expected)
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 0;
        AcsPlayerInventory.Give(player, name, 3);
        Assert.Equal(expected, player.Inventory.Ammo(ammo));
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void RepeatedWeaponGrantAddsDefaultAmmoAndPreservesPending()
    {
        var player = new PlayerPawn();
        player.Inventory.Pending = WeaponKind.Chaingun;
        AcsPlayerInventory.Give(player, "Shotgun", 1);
        AcsPlayerInventory.Give(player, "Shotgun", 50);
        Assert.Equal(16, player.Inventory.Shells);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(WeaponKind.Chaingun, player.Inventory.Pending);
    }

    [Fact]
    public void BackpackFromEmptyAmmoDoesNotQueueWeaponSwitch()
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 0;
        player.Inventory.Weapons |= WeaponKind.Chaingun | WeaponKind.Shotgun | WeaponKind.Plasma;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Backpack));
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(10, player.Inventory.Bullets);
        Assert.Equal(4, player.Inventory.Shells);
    }
}
