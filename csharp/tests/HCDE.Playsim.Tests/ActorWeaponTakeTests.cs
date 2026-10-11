using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorWeaponTakeTests
{
    [Theory]
    [InlineData("Fist", WeaponKind.Fist, 0)]
    [InlineData("Pistol", WeaponKind.Pistol, -1)]
    [InlineData("Shotgun", WeaponKind.Shotgun, 1)]
    [InlineData("SuperShotgun", WeaponKind.SuperShotgun, int.MinValue)]
    [InlineData("Chaingun", WeaponKind.Chaingun, int.MaxValue)]
    [InlineData("RocketLauncher", WeaponKind.RocketLauncher, 0)]
    [InlineData("PlasmaRifle", WeaponKind.Plasma, 1)]
    [InlineData("BFG9000", WeaponKind.Bfg, -3)]
    [InlineData("Chainsaw", WeaponKind.Chainsaw, 0)]
    public void RemovalUsesOwnershipAndPreservesAmmo(string name, WeaponKind weapon, int amount)
    {
        var player = Room().Players.First(); player.Inventory.Weapons |= weapon;
        player.Inventory.Shells = 10; player.Inventory.Rockets = 5; player.Inventory.Cells = 40;
        Assert.True(ActorInventoryActions.TakeInventory(player, name.ToLowerInvariant(), amount, flags: 1));
        Assert.False(player.Inventory.Owns(weapon));
        Assert.False(ActorInventoryActions.TakeInventory(player, name));
        Assert.Equal((50, 10, 5, 40), (player.Inventory.Bullets, player.Inventory.Shells, player.Inventory.Rockets, player.Inventory.Cells));
    }

    [Fact]
    public void ReadyRemovalRaisesReplacementAndPendingRemovalCancelsSelection()
    {
        var player = Room().Players.First(); player.Inventory.Weapons |= WeaponKind.Shotgun;
        player.Inventory.Selected = WeaponKind.Shotgun; player.Inventory.Pending = null;
        Assert.True(ActorInventoryActions.TakeInventory(player, "Shotgun"));
        Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected); Assert.Null(player.Inventory.Pending);
        Assert.Equal(122, player.WeaponOffsetY);
        player.Inventory.Weapons |= WeaponKind.Shotgun; player.Inventory.Pending = WeaponKind.Shotgun;
        Assert.True(ActorInventoryActions.TakeInventory(player, "Shotgun"));
        Assert.Null(player.Inventory.Pending); Assert.Equal(WeaponKind.Pistol, player.Inventory.Selected);
    }

    [Fact]
    public void TargetCallbackAndSavePreserveRemovedOwnership()
    {
        var sim = Room(); var player = sim.Players.First(); var caller = sim.AddBot(20, 0);
        caller.Brain!.SetSpecialTarget(player); player.Inventory.Weapons |= WeaponKind.Chainsaw;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "Chainsaw"))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.Weapons |= WeaponKind.Chainsaw; SimSavegame.Apply(sim, saved);
        Assert.False(player.Inventory.Owns(WeaponKind.Chainsaw));
        Assert.False(ActorInventoryActions.TakeInventory(player, "Pistol", pointerSelector: 1));
        Assert.False(ActorInventoryActions.TakeInventory(caller, "Pistol"));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "Pistol"));
        Assert.True(player.Inventory.Owns(WeaponKind.Pistol));
    }

    [Fact]
    public void FamilyRemovalCountsOwnersAndExcludesCallerAndUnrelatedPlayers()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(players[0], "Pistol"));
        Assert.True(players[0].Inventory.Owns(WeaponKind.Pistol)); Assert.True(players[2].Inventory.Owns(WeaponKind.Pistol));
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "Pistol"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "Pistol"));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
