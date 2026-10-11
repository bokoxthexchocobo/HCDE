using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorAmmoTakeTests
{
    [Theory]
    [InlineData("Clip", 0, 0)]
    [InlineData("Shell", -5, 15)]
    [InlineData("RocketAmmo", 5, 15)]
    [InlineData("Cell", int.MinValue, 0)]
    [InlineData("Clip", int.MaxValue, 0)]
    public void RemovalUsesPoolQuantityAndPreservesCapacity(string name, int amount, int remaining)
    {
        var player = Room().Players.First(); var inventory = player.Inventory;
        inventory.Bullets = inventory.Shells = inventory.Rockets = inventory.Cells = 20;
        var capacity = AcsPlayerInventory.AmmoCapacity(player, name);
        Assert.True(ActorInventoryActions.TakeInventory(player, name.ToLowerInvariant(), amount, flags: 1));
        Assert.Equal(remaining, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(capacity, AcsPlayerInventory.AmmoCapacity(player, name));
        Assert.Equal(60 + remaining, inventory.Bullets + inventory.Shells + inventory.Rockets + inventory.Cells);
        Assert.True(inventory.Owns(WeaponKind.Pistol));
        Assert.True(remaining == 0 || ActorInventoryActions.TakeInventory(player, name));
        Assert.False(ActorInventoryActions.TakeInventory(player, name));
    }

    [Fact]
    public void TargetStateCallbackUsesSecondaryPointer()
    {
        var sim = Room(); var player = sim.Players.First(); var caller = sim.AddBot(20, 0);
        var target = sim.AddBot(40, 0); caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "Clip", -10, pointerSelector: 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.Equal(40, player.Inventory.Bullets);
        Assert.False(ActorInventoryActions.TakeInventory(player, "Clip", pointerSelector: 1));
        Assert.False(ActorInventoryActions.TakeInventory(caller, "Clip"));
        Assert.False(ActorInventoryActions.TakeInventory(player, "ClipBox"));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "Clip"));
        Assert.Equal(40, player.Inventory.Bullets);
    }

    [Fact]
    public void FamilyRemovalCountsNonemptyPoolsAndExcludesCallerAndUnrelatedPlayers()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(players[0], "Clip"));
        Assert.Equal(50, players[0].Inventory.Bullets); Assert.Equal(50, players[2].Inventory.Bullets);
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "Clip", 10));
        Assert.Equal(40, players[0].Inventory.Bullets);
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "Clip"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "Clip"));
    }

    [Fact]
    public void DepletedPoolAndCapacitySurviveSave()
    {
        var sim = Room(); var player = sim.Players.First();
        player.Inventory.MaxBullets = 400;
        Assert.True(ActorInventoryActions.TakeInventory(player, "Clip"));
        var saved = SimSavegame.Write(sim); player.Inventory.Bullets = 50; SimSavegame.Apply(sim, saved);
        Assert.Equal(0, player.Inventory.Bullets); Assert.Equal(400, player.Inventory.MaxBullets);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
