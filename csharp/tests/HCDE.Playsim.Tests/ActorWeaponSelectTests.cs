using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWeaponSelectTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(0)]
    [InlineData(-10)]
    public void NamedSelectionRequiresOwnershipButNotAmmoOrLivingHealth(int health)
    {
        var player = Room().Players.Single(); player.Health = health;
        player.Inventory.Weapons |= WeaponKind.Shotgun; player.Inventory.Shells = 0;
        Assert.True(ActorInventoryActions.SelectWeapon(player, "sHoTgUn"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
        Assert.Equal(0, player.Inventory.Shells);
    }

    [Fact]
    public void ReadySelectionPreservesExistingPendingSwitch()
    {
        var player = Room().Players.Single(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.True(ActorInventoryActions.SelectWeapon(player, "Pistol"));
        Assert.Equal(WeaponKind.Shotgun, player.Inventory.Pending);
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void MissingUnknownNullAndInvalidActorsPreserveSelection()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Pending = WeaponKind.Fist;
        Assert.False(ActorInventoryActions.SelectWeapon(player, "Shotgun"));
        Assert.False(ActorInventoryActions.SelectWeapon(player, "Unknown"));
        Assert.False(ActorInventoryActions.SelectWeapon(player, null));
        Assert.False(ActorInventoryActions.SelectWeapon(sim.AddBot(20, 0), "Pistol"));
        player.Destroy(); Assert.False(ActorInventoryActions.SelectWeapon(player, "Pistol"));
        Assert.Equal(WeaponKind.Fist, player.Inventory.Pending);
    }

    [Fact]
    public void StateCallbackSelectionSurvivesSaveWithoutConsumingWeapon()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Weapons |= WeaponKind.Chainsaw;
        var result = false;
        player.States.Configure(player, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SelectWeapon(self, "Chainsaw"))], 0);
        player.States.Enter(player, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.Pending = null; SimSavegame.Apply(sim, saved);
        Assert.Equal(WeaponKind.Chainsaw, player.Inventory.Pending);
        Assert.True(player.Inventory.Owns(WeaponKind.Chainsaw));
        Assert.Equal(50, player.Inventory.Bullets);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
