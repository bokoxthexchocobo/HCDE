using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryFamilyTests
{
    [Fact]
    public void ChildrenActionCountsOwnersRatherThanRemovedItems()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var players = sim.Players.ToArray();
        players[0].MasterId = parent.Id; players[1].MasterId = parent.Id;
        foreach (var player in players) player.Inventory.GiveHealthPickup("MedPatch", 5);
        var unsupportedChild = sim.AddBot(40, 0); unsupportedChild.MasterId = parent.Id;
        var count = -1;
        parent.States.Configure(parent, [new(-1, 0), new(5, 0,
            Action: self => count = ActorInventoryActions.TakeFromChildren(self, "MEDPATCH", 2))], 0);
        parent.States.Enter(parent, 1);
        Assert.Equal(2, count); Assert.Equal(1, parent.States.Current);
        Assert.Equal(3, players[0].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(3, players[1].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(5, players[2].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(2, ActorInventoryActions.TakeFromChildren(parent, "MedPatch"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "MedPatch"));
    }

    [Fact]
    public void SiblingsExcludeCallerAndRequireSharedExistingMaster()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var players = sim.Players.ToArray();
        players[0].MasterId = parent.Id; players[1].MasterId = parent.Id;
        players[2].MasterId = players[0].Id;
        foreach (var player in players) player.Inventory.GiveHealthPickup("SurgeryKit", 3);
        Assert.Equal(1, ActorInventoryActions.TakeFromSiblings(players[0], "SurgeryKit", -1));
        Assert.Equal(3, players[0].Inventory.CountHealthPickup("SurgeryKit"));
        Assert.Equal(2, players[1].Inventory.CountHealthPickup("SurgeryKit"));
        Assert.Equal(3, players[2].Inventory.CountHealthPickup("SurgeryKit"));
        parent.Destroy();
        Assert.Equal(0, ActorInventoryActions.TakeFromSiblings(players[0], "SurgeryKit"));
        Assert.Equal(2, players[1].Inventory.CountHealthPickup("SurgeryKit"));
    }

    [Fact]
    public void DestroyedAndEmptyOwnersDoNotContributeToCount()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var players = sim.Players.ToArray();
        foreach (var player in players) player.MasterId = parent.Id;
        players[0].Inventory.GiveHealthPickup("ArtiHealth", 2);
        players[1].Inventory.GiveHealthPickup("ArtiHealth", 2); players[1].Destroy();
        Assert.Equal(1, ActorInventoryActions.TakeFromChildren(parent, "ArtiHealth", 1));
        Assert.Equal(1, players[0].Inventory.CountHealthPickup("ArtiHealth"));
        Assert.Equal(2, players[1].Inventory.CountHealthPickup("ArtiHealth"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, "Unknown"));
        Assert.Equal(0, ActorInventoryActions.TakeFromChildren(parent, null));
        Assert.Equal(0, ActorInventoryActions.TakeFromSiblings(parent, "ArtiHealth"));
    }

    [Fact]
    public void FamilySelectionRequiresSimulation()
    {
        var actor = new Actor();
        Assert.Throws<InvalidOperationException>(() => ActorInventoryActions.TakeFromChildren(actor, "MedPatch"));
        Assert.Throws<InvalidOperationException>(() => ActorInventoryActions.TakeFromSiblings(actor, "MedPatch"));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
