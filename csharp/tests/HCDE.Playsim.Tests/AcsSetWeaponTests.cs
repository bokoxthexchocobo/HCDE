namespace HCDE.Playsim.Tests;

public class AcsSetWeaponTests
{
    [Fact]
    public void ReadyWeaponWithEmptyAmmoCancelsPendingSwitch()
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 0;
        player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.Equal(1, AcsPlayerInventory.SetWeapon(player, "Pistol"));
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Theory]
    [InlineData("Health")]
    [InlineData("GreenArmor")]
    [InlineData("Backpack")]
    [InlineData("UnknownWeapon")]
    public void NonWeaponDoesNotRunInventoryUse(string name)
    {
        var player = new PlayerPawn { Health = 50 };
        player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.Equal(0, AcsPlayerInventory.SetWeapon(player, name));
        Assert.Equal(50, player.Health);
        Assert.Equal(0, player.Inventory.Armor);
        Assert.False(player.Inventory.HasBackpack);
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Theory]
    [InlineData(false, 8, 0)]
    [InlineData(true, 0, 0)]
    [InlineData(true, 1, 1)]
    public void DifferentWeaponRequiresOwnershipAndAmmo(bool owned, int shells, int result)
    {
        var player = new PlayerPawn();
        if (owned) player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = shells;
        player.Inventory.Pending = WeaponKind.Chaingun;
        Assert.Equal(result, AcsPlayerInventory.SetWeapon(player, "Shotgun"));
        Assert.Equal(result == 1 ? WeaponKind.Shotgun : WeaponKind.Chaingun, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void InvalidStringIndexPreservesPending(int index)
    {
        var player = new PlayerPawn();
        player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.Equal(0, AcsPlayerInventory.SetWeapon(player, ["Pistol"], index));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }
}
