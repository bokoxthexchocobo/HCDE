namespace HCDE.Playsim.Tests;

public class WeaponUseResultTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(8)]
    public void OwnedWeaponUseQueuesWithoutAmmoGateAndReturnsFalse(int shells)
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = shells;
        Assert.Equal(0, AcsPlayerInventory.Use(player, "Shotgun"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun));
        Assert.Equal(shells, player.Inventory.Shells);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void ReadyWeaponUsePreservesPendingAndReturnsFalse()
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 0;
        player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.Equal(0, AcsPlayerInventory.Use(player, "Pistol"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void SetWeaponStillRequiresAmmoAndReturnsSuccess()
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        Assert.Equal(0, AcsPlayerInventory.SetWeapon(player, "Shotgun"));
        Assert.Null(player.Inventory.Pending);
        player.Inventory.Shells = 1;
        Assert.Equal(1, AcsPlayerInventory.SetWeapon(player, "Shotgun"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }
}
