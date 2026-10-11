using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HighJumpInventoryActionTests
{
    [Fact]
    public void TakeRemovesExplicitlyTimedPowerAndReportsPresence()
    {
        var player = Room().Players.First(); player.GivePowerHighJump(99);
        Assert.True(ActorInventoryActions.CheckInventory(player, "PowerHighJump", 0));
        Assert.True(ActorInventoryActions.TakeInventory(player, "PowerHighJump", flags: 1));
        Assert.Equal(0, player.PowerHighJumpTics);
        Assert.False(ActorInventoryActions.TakeInventory(player, "PowerHighJump"));
    }

    [Fact]
    public void SetExistingItemChangesAmountWithoutRefreshingTimer()
    {
        var player = Room().Players.First(); player.GivePowerHighJump(99);
        Assert.False(ActorInventoryActions.SetInventory(player, "PowerHighJump", 1));
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerHighJump", 5));
        Assert.Equal(99, player.PowerHighJumpTics);
        Assert.Throws<NotSupportedException>(() => ActorInventoryActions.SetInventory(player, "PowerHighJump", 5, beyondMax: true));
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerHighJump", 0));
        Assert.Equal(0, player.PowerHighJumpTics);
    }

    [Fact]
    public void DefaultCreationPreservesZeroDurationAndExistingTimer()
    {
        var player = Room().Players.First();
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerHighJump"));
        Assert.True(player.PowerHighJumpPendingRemoval);
        Assert.False(ActorInventoryActions.SetInventory(player, "PowerHighJump", 1));
        Assert.True(ActorInventoryActions.SetInventory(player, "PowerHighJump", 0));
        player.GivePowerHighJump(99);
        Assert.True(ActorInventoryActions.GiveInventory(player, "PowerHighJump"));
        Assert.Equal(99, player.PowerHighJumpTics);
    }

    [Fact]
    public void FamilyTakeHandlesChildrenAndSiblingsWithoutRemovingCaller()
    {
        var sim = Room(); var players = sim.Players.ToArray();
        var parent = players[0]; var child = players[1]; child.MasterId = parent.Id;
        parent.GivePowerHighJump(90); child.GivePowerHighJump(80);
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "PowerHighJump"));
        Assert.Equal(90, parent.PowerHighJumpTics); Assert.Equal(0, child.PowerHighJumpTics);
        child.GivePowerHighJump(80); parent.MasterId = parent.Id;
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(parent, "PowerHighJump"));
        Assert.Equal(90, parent.PowerHighJumpTics); Assert.Equal(0, child.PowerHighJumpTics);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 400 }],
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
