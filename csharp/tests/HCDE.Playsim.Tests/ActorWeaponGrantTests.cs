using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWeaponGrantTests
{
    [Theory]
    [InlineData("Fist", WeaponKind.Fist, 0, 0, 0, 0)]
    [InlineData("Pistol", WeaponKind.Pistol, 20, 0, 0, 0)]
    [InlineData("Shotgun", WeaponKind.Shotgun, 0, 8, 0, 0)]
    [InlineData("SuperShotgun", WeaponKind.SuperShotgun, 0, 8, 0, 0)]
    [InlineData("Chaingun", WeaponKind.Chaingun, 20, 0, 0, 0)]
    [InlineData("RocketLauncher", WeaponKind.RocketLauncher, 0, 0, 2, 0)]
    [InlineData("PlasmaRifle", WeaponKind.Plasma, 0, 0, 0, 40)]
    [InlineData("BFG9000", WeaponKind.Bfg, 0, 0, 0, 40)]
    [InlineData("Chainsaw", WeaponKind.Chainsaw, 0, 0, 0, 0)]
    public void GrantCountDoesNotMultiplyWeaponAmmo(string name, WeaponKind kind, int bullets, int shells, int rockets, int cells)
    {
        var player = Room().Players.First(); var inventory = player.Inventory;
        inventory.Weapons = 0; inventory.Selected = 0; inventory.Pending = null;
        inventory.Bullets = inventory.Shells = inventory.Rockets = inventory.Cells = 0;
        Assert.True(ActorInventoryActions.GiveInventory(player, name.ToLowerInvariant(), 100));
        Assert.True(inventory.Owns(kind)); Assert.Equal(kind, inventory.Pending);
        Assert.Equal((bullets, shells, rockets, cells), (inventory.Bullets, inventory.Shells, inventory.Rockets, inventory.Cells));
        inventory.Bullets = inventory.MaxBullets; inventory.Shells = inventory.MaxShells;
        inventory.Rockets = inventory.MaxRockets; inventory.Cells = inventory.MaxCells;
        Assert.False(ActorInventoryActions.GiveInventory(player, name));
    }

    [Fact]
    public void TargetCallbackUsesMasterAndDuplicateGrantProvidesAmmo()
    {
        var sim = Room(); var player = sim.Players.First(); var caller = sim.AddBot(20, 0);
        var target = sim.AddBot(40, 0); caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "Shotgun", -1, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.Equal(8, player.Inventory.Shells);
        Assert.True(ActorInventoryActions.GiveInventory(player, "Shotgun", 0)); Assert.Equal(16, player.Inventory.Shells);
        Assert.False(ActorInventoryActions.GiveInventory(player, "Shotgun", pointerSelector: 1));
        Assert.False(ActorInventoryActions.GiveInventory(caller, "Shotgun"));
        player.Destroy(); Assert.False(ActorInventoryActions.GiveInventory(player, "Shotgun"));
        Assert.Equal(16, player.Inventory.Shells);
    }

    [Fact]
    public void FamilyGrantCountsSuccessfulPickupsAndSavePreservesWeapon()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id;
        Assert.Equal(2, ActorInventoryActions.GiveToChildren(parent, "Chainsaw"));
        Assert.False(players[2].Inventory.Owns(WeaponKind.Chainsaw));
        Assert.Equal(0, ActorInventoryActions.GiveToSiblings(players[0], "Chainsaw"));
        Assert.Equal(1, ActorInventoryActions.GiveToSiblings(players[0], "RocketLauncher"));
        Assert.False(players[0].Inventory.Owns(WeaponKind.RocketLauncher));
        var saved = SimSavegame.Write(sim); players[1].Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.True(players[1].Inventory.Owns(WeaponKind.RocketLauncher)); Assert.Equal(2, players[1].Inventory.Rockets);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
