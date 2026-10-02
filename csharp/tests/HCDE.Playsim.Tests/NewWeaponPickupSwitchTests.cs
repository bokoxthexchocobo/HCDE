using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NewWeaponPickupSwitchTests
{
    [Fact]
    public void ContactPickupUsesExistingWeaponLowerAndRaiseTransition()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }, new LevelThing { Type = PickupCatalog.Shotgun }],
        });
        var player = sim.Players.Single(); sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        for (var i = 0; i < 40; i++) sim.Tick();
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Selected); Assert.Null(player.Inventory.Pending);
    }

    [Fact]
    public void NeverSwitchPreferenceAffectsChecksum()
    {
        var level = new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] };
        var first = AuthoritySimulation.Start(level); var second = AuthoritySimulation.Start(level);
        second.Players.Single().Inventory.NeverAutoSwitch = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }
    [Theory]
    [InlineData(PickupCatalog.Shotgun, WeaponKind.Shotgun)]
    [InlineData(PickupCatalog.Chaingun, WeaponKind.Chaingun)]
    [InlineData(PickupCatalog.Chainsaw, WeaponKind.Chainsaw)]
    [InlineData(PickupCatalog.Bfg, WeaponKind.Bfg)]
    public void NewlyAcquiredWeaponQueuesSwitchWithoutReplacingReadyWeapon(int type, WeaponKind weapon)
    {
        var player = new PlayerPawn();
        Assert.True(PickupCatalog.TryGive(player, type));
        Assert.True(player.Inventory.Owns(weapon));
        Assert.Equal(weapon, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void NeverSwitchPreferenceStillGrantsWeaponAndAmmo()
    {
        var player = new PlayerPawn(); player.Inventory.NeverAutoSwitch = true;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shotgun));
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun)); Assert.Equal(8, player.Inventory.Shells);
        Assert.Null(player.Inventory.Pending); Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void AlreadyOwnedWeaponDoesNotQueueNewWeaponSwitch()
    {
        var player = new PlayerPawn(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = 1;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shotgun));
        Assert.Null(player.Inventory.Pending); Assert.Equal(9, player.Inventory.Shells);
    }

    [Fact]
    public void NewAcquisitionReplacesPriorPendingWeapon()
    {
        var player = new PlayerPawn(); player.Inventory.Pending = WeaponKind.Chaingun;
        Assert.True(PickupCatalog.TryGive(player, PickupCatalog.Shotgun));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
    }
}
