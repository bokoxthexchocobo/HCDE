namespace HCDE.Playsim.Tests;

public class WeaponRemovalLifecycleTests
{
    [Fact]
    public void RemovedPendingWeaponCancelsSwitchWithoutChangingReadyWeapon()
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Pending = WeaponKind.Shotgun;
        AcsPlayerInventory.Take(player, "Shotgun", 1);
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void RemovedReadyWeaponRaisesBestUsableReplacementDespiteNeverSwitch()
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons |= WeaponKind.Shotgun | WeaponKind.SuperShotgun | WeaponKind.Bfg;
        player.Inventory.Selected = WeaponKind.Shotgun;
        player.Inventory.Shells = 8; player.Inventory.Cells = 0;
        player.Inventory.NeverAutoSwitch = true;
        AcsPlayerInventory.Take(player, "Shotgun", 1);
        Assert.Equal(WeaponKind.SuperShotgun, player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.Equal(PlayerPawn.WeaponBottom - PlayerPawn.WeaponMoveSpeed, player.WeaponOffsetY);
        Assert.False(player.Inventory.Owns(WeaponKind.Shotgun));
    }

    [Fact]
    public void ReadyRemovalPreservesDifferentPendingWeapon()
    {
        var player = new PlayerPawn(); player.Inventory.Pending = WeaponKind.Fist;
        AcsPlayerInventory.Take(player, "Pistol", 1);
        Assert.Equal(default(WeaponKind), player.Inventory.Selected);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
    }

    [Fact]
    public void RemovingLastWeaponLeavesNoReadyOrPendingWeapon()
    {
        var player = new PlayerPawn(); player.Inventory.Weapons = WeaponKind.Pistol;
        AcsPlayerInventory.Take(player, "Pistol", 1);
        Assert.Equal(default(WeaponKind), player.Inventory.Selected);
        Assert.Null(player.Inventory.Pending);
        Assert.False(player.WeaponReady);
    }

    [Fact]
    public void RemovingUnownedWeaponDoesNotDisturbSelection()
    {
        var player = new PlayerPawn(); player.Inventory.Pending = WeaponKind.Fist;
        AcsPlayerInventory.Take(player, "Shotgun", 1);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
    }
}
