using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryCheckTests
{
    [Theory]
    [InlineData(10, true)]
    [InlineData(11, false)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public void AmmoUsesQuantityOrFullCapacity(int amount, bool expected)
    {
        var player = new PlayerPawn();
        player.Inventory.Bullets = 10;
        Assert.Equal(expected, ActorInventoryActions.CheckInventory(player, "Clip", amount));
        player.Inventory.Bullets = player.Inventory.MaxBullets;
        Assert.True(ActorInventoryActions.CheckInventory(player, "Clip", 0));
    }

    [Fact]
    public void HeldHealthUsesStackMaximumAndImmediateHealthIsNotHeld()
    {
        var player = new PlayerPawn();
        ActorInventoryActions.SetInventory(player, "MedPatch", 20);
        Assert.True(ActorInventoryActions.CheckInventory(player, "MedPatch", -3));
        Assert.False(ActorInventoryActions.CheckInventory(player, "MedPatch", 21));
        Assert.False(ActorInventoryActions.CheckInventory(player, "Health", 1));
        Assert.False(ActorInventoryActions.CheckInventory(player, "HealthBonus", 0));
        Assert.False(ActorInventoryActions.CheckInventory(player, "Unknown", 0));
    }

    [Fact]
    public void StateCallbackQueriesPointerSelectedInventoryWithoutMutation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        var player = sim.Players.Single();
        var caller = sim.AddBot(20, 0);
        caller.MasterId = player.Id;
        ActorInventoryActions.GiveInventory(player, "PowerBuddha");
        var duration = player.PowerBuddhaTics;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.CheckInventory(self, "PowerBuddha", 0, 4))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result);
        Assert.Equal(duration, player.PowerBuddhaTics);
        Assert.False(ActorInventoryActions.CheckInventory(caller, "PowerBuddha", 1));
        Assert.False(ActorInventoryActions.CheckInventory(caller, "PowerBuddha", 1, 1));
        player.Destroy();
        Assert.False(ActorInventoryActions.CheckInventory(caller, "PowerBuddha", 1, 4));
    }
}
