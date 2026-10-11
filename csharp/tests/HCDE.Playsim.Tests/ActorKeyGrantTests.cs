using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKeyGrantTests
{
    [Theory]
    [InlineData("BlueCard")]
    [InlineData("BlueSkull")]
    [InlineData("RedCard")]
    [InlineData("RedSkull")]
    [InlineData("YellowCard")]
    [InlineData("YellowSkull")]
    public void GrantsSetOwnershipAndDuplicateGrantsFail(string name)
    {
        var player = Room().Players.First();
        Assert.True(ActorInventoryActions.GiveInventory(player, name.ToLowerInvariant(), 100));
        Assert.Equal(1, AcsPlayerInventory.Count(player, name, false));
        Assert.False(ActorInventoryActions.GiveInventory(player, name));
        Assert.Equal(100, player.Health); Assert.Empty(player.Inventory.HealthPickups);
    }

    [Fact]
    public void TargetStateGrantUsesDefaultQuantity()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.First();
        caller.Brain!.SetSpecialTarget(player); var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "BlueCard", -1))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.True(player.Inventory.BlueKey); Assert.Equal(1, caller.States.Current);
    }

    [Fact]
    public void FamilyCountsNewOwnersAndPreservesUnrelatedPlayers()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var players = sim.Players.ToArray();
        players[0].MasterId = parent.Id; players[1].MasterId = parent.Id;
        players[0].Inventory.RedKey = true;
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "RedCard"));
        Assert.False(players[2].Inventory.RedKey);
        Assert.Equal(1, ActorInventoryActions.GiveToSiblings(players[0], "YellowCard"));
        Assert.False(players[0].Inventory.YellowKey); Assert.True(players[1].Inventory.YellowKey);
        Assert.False(ActorInventoryActions.GiveInventory(parent, "RedCard"));
        players[1].Destroy(); Assert.Equal(0, ActorInventoryActions.GiveToSiblings(players[0], "BlueCard"));
    }

    [Fact]
    public void SinglePlayerDuplicateClassGrantStillReportsSuccess()
    {
        var player = Room(SpawnGameMode.Single).Players.First();
        Assert.True(ActorInventoryActions.GiveInventory(player, "BlueCard"));
        Assert.True(ActorInventoryActions.GiveInventory(player, "BlueCard"));
        Assert.True(player.Inventory.BlueKey);
    }

    private static AuthoritySimulation Room(SpawnGameMode mode = SpawnGameMode.Cooperative) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: mode));
}
