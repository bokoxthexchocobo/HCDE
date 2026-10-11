using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryTakeTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(-2, 3)]
    [InlineData(2, 3)]
    [InlineData(int.MinValue, 0)]
    [InlineData(int.MaxValue, 0)]
    public void ActionRemovalUsesNativeDepletionWithoutHealing(int amount, int remaining)
    {
        var player = Room().Players.Single(); player.Health = 30;
        player.Inventory.GiveHealthPickup("MedPatch", 5);
        Assert.True(ActorInventoryActions.TakeInventory(player, "MEDPATCH", amount, flags: 1));
        Assert.Equal(remaining, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(30, player.Health);
    }

    [Fact]
    public void StateCallbackTakesFromTargetAndOnlyFirstMatchingStack()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.Single();
        caller.Brain!.SetSpecialTarget(player);
        player.Inventory.AddHealthPickup(10, 2, 3, "MedPatch");
        player.Inventory.AddHealthPickup(25, 3, 0, "MedPatch");
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.TakeFromTarget(self, "MedPatch"))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result); Assert.Equal(1, caller.States.Current);
        Assert.Equal(new HealthPickupStack(25, 3, 0, "MedPatch"), Assert.Single(player.Inventory.HealthPickups));
    }

    [Fact]
    public void TargetWrapperResolvesSecondaryPointerFromTarget()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var target = sim.AddBot(40, 0);
        var player = sim.Players.Single(); caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        player.Inventory.GiveHealthPickup("SurgeryKit", 3);
        Assert.True(ActorInventoryActions.TakeFromTarget(caller, "SurgeryKit", -1, pointerSelector: 4));
        Assert.Equal(2, player.Inventory.CountHealthPickup("SurgeryKit"));
        Assert.False(ActorInventoryActions.TakeFromTarget(target, "SurgeryKit", 1, pointerSelector: 0x40));
    }

    [Fact]
    public void MissingUnsupportedAndNullSelectionsPreserveInventory()
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.GiveHealthPickup("ArtiHealth", 2);
        Assert.False(ActorInventoryActions.TakeInventory(player, "ArtiHealth", pointerSelector: 1));
        Assert.False(ActorInventoryActions.TakeInventory(player, "ArtiSuperHealth"));
        Assert.False(ActorInventoryActions.TakeInventory(player, "Unknown"));
        Assert.False(ActorInventoryActions.TakeInventory(player, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorInventoryActions.TakeInventory(player, "ArtiHealth", flags: 2));
        AcsPlayerInventory.Take(player, "ArtiHealth", 0); AcsPlayerInventory.Take(player, "ArtiHealth", -1);
        Assert.Equal(2, player.Inventory.CountHealthPickup("ArtiHealth"));
        player.Destroy(); Assert.False(ActorInventoryActions.TakeInventory(player, "ArtiHealth"));
        Assert.Equal(2, player.Inventory.CountHealthPickup("ArtiHealth"));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
