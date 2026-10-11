using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWeaponPriorityTests
{
    [Fact]
    public void FallbackSkipsEmptyAmmoAndLowersReadyWeapon()
    {
        var player = Room().Players.Single();
        player.Inventory.Weapons |= WeaponKind.Plasma | WeaponKind.Shotgun;
        player.Inventory.Cells = 0; player.Inventory.Shells = 1;
        Assert.True(ActorInventoryActions.SelectWeapon(player, "None", 1));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending); Assert.True(player.WeaponLowering);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void NamedOwnedWeaponTakesPrecedenceEvenWithoutAmmo()
    {
        var player = Room().Players.Single(); player.Inventory.Weapons |= WeaponKind.Plasma;
        Assert.True(ActorInventoryActions.SelectWeapon(player, "PlasmaRifle", 1));
        Assert.Equal(WeaponKind.Plasma, player.Inventory.Pending); Assert.False(player.WeaponLowering);
    }

    [Fact]
    public void NoReadyWeaponRaisesBestAndNoUsableWeaponStillReportsSuccess()
    {
        var player = Room().Players.Single(); player.Inventory.Selected = 0;
        Assert.True(ActorInventoryActions.SelectWeapon(player, null, 1));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected); Assert.Null(player.Inventory.Pending);
        player.Inventory.Weapons = 0; player.Inventory.Selected = 0;
        player.Inventory.Pending = WeaponKind.Chainsaw;
        Assert.True(ActorInventoryActions.SelectWeapon(player, "Missing", 1));
        Assert.Equal(WeaponKind.Chainsaw, player.Inventory.Pending);
    }

    [Fact]
    public void PriorityLoweringSurvivesSaveAndInvalidFlagsFailExplicitly()
    {
        var sim = Room(); var player = sim.Players.Single();
        Assert.True(ActorInventoryActions.SelectWeapon(player, null, 1));
        var saved = SimSavegame.Write(sim); player.WeaponLowering = false; SimSavegame.Apply(sim, saved);
        Assert.True(player.WeaponLowering); Assert.Equal(WeaponKind.Pistol, player.Inventory.Pending);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorInventoryActions.SelectWeapon(player, null, 2));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
