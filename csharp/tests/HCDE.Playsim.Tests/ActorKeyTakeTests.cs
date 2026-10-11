using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorKeyTakeTests
{
    [Theory]
    [InlineData("BlueCard", 0)]
    [InlineData("BlueSkull", -1)]
    [InlineData("RedCard", 1)]
    [InlineData("RedSkull", int.MinValue)]
    [InlineData("YellowCard", int.MaxValue)]
    [InlineData("YellowSkull", 0)]
    public void RemovalReportsHeldKeyAndPreservesOtherColors(string name, int amount)
    {
        var player = Room().Players.First();
        player.Inventory.BlueKey = player.Inventory.RedKey = player.Inventory.YellowKey = true;
        Assert.True(ActorInventoryActions.TakeInventory(player, name.ToLowerInvariant(), amount, flags: 1));
        Assert.Equal(0, AcsPlayerInventory.Count(player, name, false));
        Assert.Equal(2, (player.Inventory.BlueKey ? 1 : 0) + (player.Inventory.RedKey ? 1 : 0)
            + (player.Inventory.YellowKey ? 1 : 0));
        Assert.False(ActorInventoryActions.TakeInventory(player, name));
    }

    [Fact]
    public void TargetCallbackUsesSecondaryPointerAndRejectsInvalidReceivers()
    {
        var sim = Room(); var player = sim.Players.First(); var caller = sim.AddBot(20, 0);
        var target = sim.AddBot(40, 0); caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        player.Inventory.BlueKey = true; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "BlueCard", pointerSelector: 4))], 0);
        caller.States.Enter(caller, 1); Assert.True(result); Assert.False(player.Inventory.BlueKey);
        player.Inventory.BlueKey = true;
        Assert.False(ActorInventoryActions.TakeInventory(player, "BlueCard", pointerSelector: 1));
        Assert.False(ActorInventoryActions.TakeInventory(caller, "BlueCard"));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "BlueCard"));
        Assert.True(player.Inventory.BlueKey);
    }

    [Fact]
    public void FamilyRemovalCountsOwnersAndExcludesCallerAndUnrelatedPlayers()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id;
        foreach (var player in players) player.Inventory.RedKey = true;
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(players[0], "RedCard"));
        Assert.True(players[0].Inventory.RedKey); Assert.True(players[2].Inventory.RedKey);
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "RedSkull"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "RedCard"));
        Assert.True(players[2].Inventory.RedKey);
    }

    [Fact]
    public void RemovedKeyStaysRemovedAfterSaveAndCanBeGrantedAgain()
    {
        var sim = Room(); var player = sim.Players.First(); player.Inventory.YellowKey = true;
        Assert.True(ActorInventoryActions.TakeInventory(player, "YellowCard"));
        var saved = SimSavegame.Write(sim); player.Inventory.YellowKey = true; SimSavegame.Apply(sim, saved);
        Assert.False(player.Inventory.YellowKey);
        Assert.True(ActorInventoryActions.GiveInventory(player, "YellowCard"));
        Assert.True(player.Inventory.YellowKey);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative, InfiniteStoredInventoryUse: true));
}
