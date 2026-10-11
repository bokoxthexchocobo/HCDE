using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorAmmoGrantTests
{
    [Theory]
    [InlineData("Clip", "Clip")]
    [InlineData("ClipBox", "Clip")]
    [InlineData("Shell", "Shell")]
    [InlineData("ShellBox", "Shell")]
    [InlineData("RocketAmmo", "RocketAmmo")]
    [InlineData("RocketBox", "RocketAmmo")]
    [InlineData("Cell", "Cell")]
    [InlineData("CellPack", "Cell")]
    public void GrantAssignsRequestedUnitsInsteadOfMultiplyingPickupDefault(string name, string pool)
    {
        var player = Room().Players.First(); AcsPlayerInventory.Take(player, pool, int.MaxValue);
        Assert.True(ActorInventoryActions.GiveInventory(player, name.ToLowerInvariant(), 3));
        Assert.Equal(3, AcsPlayerInventory.Count(player, pool, false));
        Assert.True(ActorInventoryActions.GiveInventory(player, name, 0));
        Assert.True(ActorInventoryActions.GiveInventory(player, name, -5));
        Assert.Equal(5, AcsPlayerInventory.Count(player, pool, false));
    }

    [Fact]
    public void GrantUsesSkillScalingCapacityAndFullPoolFailure()
    {
        var player = Room(0).Players.First(); player.Inventory.Bullets = 0;
        Assert.True(ActorInventoryActions.GiveInventory(player, "ClipBox", 3));
        Assert.Equal(6, player.Inventory.Bullets);
        Assert.True(ActorInventoryActions.GiveInventory(player, "Clip", 150));
        Assert.Equal(200, player.Inventory.Bullets);
        Assert.False(ActorInventoryActions.GiveInventory(player, "Clip", 1));
    }

    [Fact]
    public void TargetStateCallbackSelectsMasterAndRejectsInvalidReceivers()
    {
        var sim = Room(); var player = sim.Players.First(); var caller = sim.AddBot(20, 0);
        var target = sim.AddBot(40, 0); caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "CellPack", 7, 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.Equal(7, player.Inventory.Cells);
        Assert.False(ActorInventoryActions.GiveInventory(player, "Cell", 5, 1));
        Assert.False(ActorInventoryActions.GiveInventory(caller, "Cell", 5));
        player.Destroy(); Assert.False(ActorInventoryActions.GiveInventory(player, "Cell", 5));
        Assert.Equal(7, player.Inventory.Cells);
    }

    [Fact]
    public void FamilyGrantCountsAvailableRecipientsAndPreservesUnrelatedPlayer()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id;
        players[0].Inventory.Shells = 50;
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "ShellBox", 5));
        Assert.Equal(5, players[1].Inventory.Shells); Assert.Equal(0, players[2].Inventory.Shells);
        Assert.Equal(1, ActorInventoryActions.GiveToSiblings(players[0], "Shell", 2));
        Assert.Equal(7, players[1].Inventory.Shells); Assert.Equal(50, players[0].Inventory.Shells);
    }

    private static AuthoritySimulation Room(int skill = 2) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Skill: skill, Mode: SpawnGameMode.Cooperative));
}
