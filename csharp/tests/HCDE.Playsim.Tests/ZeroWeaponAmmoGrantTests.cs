using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ZeroWeaponAmmoGrantTests
{
    [Theory]
    [InlineData(PickupCatalog.Shotgun, WeaponKind.Shotgun)]
    [InlineData(PickupCatalog.SuperShotgun, WeaponKind.SuperShotgun)]
    [InlineData(PickupCatalog.Chaingun, WeaponKind.Chaingun)]
    [InlineData(PickupCatalog.RocketLauncher, WeaponKind.RocketLauncher)]
    [InlineData(PickupCatalog.PlasmaRifle, WeaponKind.Plasma)]
    [InlineData(PickupCatalog.Bfg, WeaponKind.Bfg)]
    public void OwnedAmmoWeaponAcceptsZeroScaledGrant(int pickup, WeaponKind weapon)
    {
        var sim = Room();
        sim.AmmoFactor = 0;
        var player = sim.Players.Single();
        player.Inventory.Weapons |= weapon;
        Assert.True(PickupCatalog.TryGive(player, pickup));
        Assert.Equal(50, player.Inventory.Bullets);
        Assert.Equal(0, player.Inventory.Shells);
        Assert.Equal(0, player.Inventory.Rockets);
        Assert.Equal(0, player.Inventory.Cells);
        Assert.Null(player.Inventory.Pending);
    }

    [Fact]
    public void OwnedChainsawHasNoAmmoGrantEvenWithExplicitPickupAmount()
    {
        var player = new PlayerPawn();
        player.Inventory.Weapons |= WeaponKind.Chainsaw;
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.Chainsaw, pickupAmount: 10));
        Assert.Equal(50, player.Inventory.Bullets);
    }

    [Fact]
    public void FullAmmoStillRejectsOwnedZeroScaledWeaponPickup()
    {
        var sim = Room();
        sim.AmmoFactor = 0;
        var player = sim.Players.Single();
        player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Shells = player.Inventory.MaxShells;
        Assert.False(PickupCatalog.TryGive(player, PickupCatalog.Shotgun));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }],
    });
}
