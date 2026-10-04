namespace HCDE.Playsim.Tests;

public class AmmoSwitchWithoutReadyTests
{
    [Fact]
    public void AmmoPickupQueuesOwnedWeaponWhenNoWeaponIsReady()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        player.Inventory.Weapons = WeaponKind.Shotgun;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shells));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }

    [Fact]
    public void MatchingAmmoSelectionUsesNativePriority()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        player.Inventory.Weapons = WeaponKind.Shotgun | WeaponKind.SuperShotgun;
        PickupCatalog.TryGive(player, PickupCatalog.Shells);
        Assert.Equal(WeaponKind.SuperShotgun, player.Inventory.Pending);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void PreferenceAndExistingPendingWeaponPreventSwitch(bool neverSwitch, bool pending)
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        player.Inventory.Weapons = WeaponKind.Shotgun;
        player.Inventory.NeverAutoSwitch = neverSwitch;
        player.Inventory.Pending = pending ? WeaponKind.Fist : null;
        PickupCatalog.TryGive(player, PickupCatalog.Shells);
        Assert.Equal(pending ? WeaponKind.Fist : (WeaponKind?)null, player.Inventory.Pending);
    }

    [Fact]
    public void NoMatchingUsableWeaponDoesNotQueueSelection()
    {
        var player = new PlayerPawn();
        AcsPlayerInventory.Clear(player);
        player.Inventory.Weapons = WeaponKind.Bfg;
        PickupCatalog.TryGive(player, PickupCatalog.Cell);
        Assert.Null(player.Inventory.Pending);
        PickupCatalog.TryGive(player, PickupCatalog.Shells);
        Assert.Null(player.Inventory.Pending);
    }
}
