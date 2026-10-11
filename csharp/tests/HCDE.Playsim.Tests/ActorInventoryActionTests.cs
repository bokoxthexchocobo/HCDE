using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryActionTests
{
    [Fact]
    public void StateCallbackSetsTargetInventoryAndRetainsCallerFrame()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.Single();
        player.Health = 30; caller.Brain!.SetSpecialTarget(player);
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "MedPatch", 4, 2))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result); Assert.Equal(4, player.Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(30, player.Health); Assert.Equal(1, caller.States.Current);
        Assert.False(ActorInventoryActions.SetInventory(caller, "MedPatch", 4, 2));
        Assert.True(ActorInventoryActions.SetInventory(caller, "MedPatch", 0, 2));
        Assert.Empty(player.Inventory.HealthPickups);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(0x40)]
    public void PointerSelectedPlayerReceivesMaximumOverride(int selector)
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.Single();
        caller.MasterId = player.Id;
        player.Inventory.GiveHealthPickup("MedPatch", 1);
        Assert.True(ActorInventoryActions.SetInventory(caller, "MEDPATCH", 40, selector, beyondMax: true));
        Assert.Equal(40, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void NullMissingDestroyedAndUnsupportedSubjectsFailWithoutMutation()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var player = sim.Players.Single();
        player.Inventory.GiveHealthPickup("MedPatch", 2);
        Assert.False(ActorInventoryActions.SetInventory(player, "MedPatch", 3, 1));
        Assert.False(ActorInventoryActions.SetInventory(caller, "MedPatch", 3, 2));
        Assert.False(ActorInventoryActions.SetInventory(caller, "MedPatch", 3));
        Assert.False(ActorInventoryActions.SetInventory(player, null, 3));
        Assert.False(ActorInventoryActions.SetInventory(player, "Unknown", 3));
        caller.MasterId = player.Id; player.Destroy();
        Assert.False(ActorInventoryActions.SetInventory(caller, "MedPatch", 3, 4));
        Assert.False(ActorInventoryActions.SetInventory(player, "MedPatch", 3));
        Assert.Equal(2, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void DefaultSelectorWorksWithoutSimulationButPointerSelectionRequiresOne()
    {
        var player = new PlayerPawn();
        Assert.True(ActorInventoryActions.SetInventory(player, "SurgeryKit", 2));
        Assert.Equal(2, player.Inventory.CountHealthPickup("SurgeryKit"));
        Assert.Throws<InvalidOperationException>(() => ActorInventoryActions.SetInventory(player, "SurgeryKit", 3, 4));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
