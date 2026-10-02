namespace HCDE.Playsim.Tests;

public class AmmoPickupSwitchTests
{
    [Fact]
    public void OwnedWeaponPickupCanSwitchWhenItsAmmoWasEmpty()
    {
        var player = new PlayerPawn(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shotgun));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }
    [Theory]
    [InlineData(PickupCatalog.Clip, WeaponKind.Chaingun, WeaponKind.Chaingun)]
    [InlineData(PickupCatalog.Shells, WeaponKind.Shotgun | WeaponKind.SuperShotgun, WeaponKind.SuperShotgun)]
    [InlineData(PickupCatalog.Cell, WeaponKind.Plasma | WeaponKind.Bfg, WeaponKind.Plasma)]
    [InlineData(PickupCatalog.Shells, WeaponKind.Shotgun, WeaponKind.Shotgun)]
    public void EmptyAmmoPickupQueuesBestUsableMatchingWeapon(int type, WeaponKind owned, WeaponKind expected)
    {
        var player = new PlayerPawn(); player.Inventory.Bullets = 0;
        player.Inventory.Weapons |= owned;
        Assert.True(PickupCatalog.TryGive(player, type)); Assert.Equal(expected, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Theory]
    [InlineData(true, false, WeaponKind.Pistol, 0)]
    [InlineData(false, true, WeaponKind.Pistol, 0)]
    [InlineData(false, false, WeaponKind.Shotgun, 0)]
    [InlineData(false, false, WeaponKind.Pistol, 1)]
    public void SwitchingRequiresEmptyAmmoWimpyReadyAndNoPendingOrPreferenceBlock(bool never, bool pending, WeaponKind ready, int bullets)
    {
        var player = new PlayerPawn(); var inventory = player.Inventory;
        inventory.Weapons |= WeaponKind.Chaingun | WeaponKind.Shotgun;
        inventory.Bullets = bullets; inventory.Selected = ready; inventory.NeverAutoSwitch = never;
        inventory.Pending = pending ? WeaponKind.Shotgun : null;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Clip));
        Assert.Equal(pending ? WeaponKind.Shotgun : (WeaponKind?)null, inventory.Pending);
    }

    [Fact]
    public void InsufficientAmmoDoesNotChooseWeapon()
    {
        var player = new PlayerPawn(); player.Inventory.Weapons |= WeaponKind.SuperShotgun;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shells, pickupAmount: 1));
        Assert.Null(player.Inventory.Pending);
    }
}
