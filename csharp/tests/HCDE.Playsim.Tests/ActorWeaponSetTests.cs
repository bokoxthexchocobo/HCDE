using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWeaponSetTests
{
    [Theory]
    [InlineData("Fist", WeaponKind.Fist)]
    [InlineData("Pistol", WeaponKind.Pistol)]
    [InlineData("Shotgun", WeaponKind.Shotgun)]
    [InlineData("SuperShotgun", WeaponKind.SuperShotgun)]
    [InlineData("Chaingun", WeaponKind.Chaingun)]
    [InlineData("RocketLauncher", WeaponKind.RocketLauncher)]
    [InlineData("PlasmaRifle", WeaponKind.Plasma)]
    [InlineData("BFG9000", WeaponKind.Bfg)]
    [InlineData("Chainsaw", WeaponKind.Chainsaw)]
    public void SetterCreatesAndDepletesRepresentedWeapon(string name, WeaponKind weapon)
    {
        var player = Room().Players.Single(); player.Inventory.Weapons &= ~weapon;
        Assert.True(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.False(player.Inventory.Owns(weapon));
        Assert.True(ActorInventoryActions.SetInventory(player, name.ToLowerInvariant(), 1));
        Assert.True(player.Inventory.Owns(weapon));
        var ammo = (player.Inventory.Bullets, player.Inventory.Shells, player.Inventory.Rockets, player.Inventory.Cells);
        Assert.False(ActorInventoryActions.SetInventory(player, name, 1));
        Assert.True(ActorInventoryActions.SetInventory(player, name, 100));
        Assert.Equal(ammo, (player.Inventory.Bullets, player.Inventory.Shells, player.Inventory.Rockets, player.Inventory.Cells));
        Assert.True(ActorInventoryActions.SetInventory(player, name, -1));
        Assert.False(player.Inventory.Owns(weapon));
        Assert.Equal(ammo, (player.Inventory.Bullets, player.Inventory.Shells, player.Inventory.Rockets, player.Inventory.Cells));
    }

    [Fact]
    public void CreationIgnoresSkillScalingAndExistingBeyondMaximumFailsBeforeMutation()
    {
        var player = Room(0).Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, "Shotgun", 100));
        Assert.Equal(8, player.Inventory.Shells);
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.SetInventory(player, "Shotgun", 2, beyondMax: true));
        Assert.True(player.Inventory.Owns(WeaponKind.Shotgun)); Assert.Equal(8, player.Inventory.Shells);
    }

    [Fact]
    public void SetterDepletesReadyWeaponAndRaisesReplacement()
    {
        var player = Room().Players.Single(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun; player.Inventory.Pending = null;
        Assert.True(ActorInventoryActions.SetInventory(player, "Shotgun", 0));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected); Assert.Null(player.Inventory.Pending);
        Assert.Equal(122, player.WeaponOffsetY);
    }

    [Fact]
    public void StateCallbackMasterSelectionAndSavePreserveCreatedWeapon()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "Chainsaw", 1, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.True(player.Inventory.Owns(WeaponKind.Chainsaw));
        Assert.False(ActorInventoryActions.SetInventory(player, "Chainsaw", 0, pointerSelector: 1));
        Assert.False(ActorInventoryActions.SetInventory(caller, "Chainsaw", 1));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "Chainsaw", 0));
        Assert.True(player.Inventory.Owns(WeaponKind.Chainsaw));
    }

    private static AuthoritySimulation Room(int skill = 2) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] },
        spawnOptions: new SpawnOptions(Skill: skill));
}
