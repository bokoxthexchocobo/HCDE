using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorInventoryGiveTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(-10, 1)]
    [InlineData(4, 4)]
    [InlineData(int.MaxValue, 20)]
    public void GrantsUseNativeDefaultAndPickupCap(int amount, int expected)
    {
        var player = Room().Players.First(); player.Health = 30;
        Assert.True(ActorInventoryActions.GiveInventory(player, "MEDPATCH", amount));
        Assert.Equal(expected, player.Inventory.CountHealthPickup("MedPatch")); Assert.Equal(30, player.Health);
        Assert.True(ActorInventoryActions.GiveInventory(player, "MedPatch", int.MaxValue) || expected == 20);
        Assert.False(ActorInventoryActions.GiveInventory(player, "MedPatch"));
        Assert.Equal(20, player.Inventory.CountHealthPickup("MedPatch"));
    }

    [Fact]
    public void StateCallbackResolvesTargetThenSecondaryMaster()
    {
        var sim = Room(); var caller = sim.AddBot(20, 0); var target = sim.AddBot(40, 0); var player = sim.Players.First();
        caller.Brain!.SetSpecialTarget(target); target.MasterId = player.Id;
        var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "SurgeryKit", 2, 4))], 0);
        caller.States.Enter(caller, 1);
        Assert.True(result); Assert.Equal(1, caller.States.Current);
        Assert.Equal(new HealthPickupStack(-100, 2, 0, "SurgeryKit"), Assert.Single(player.Inventory.HealthPickups));
        Assert.False(ActorInventoryActions.GiveToTarget(target, "SurgeryKit", 2, 0x40));
        Assert.False(ActorInventoryActions.GiveInventory(player, "SurgeryKit", 2, 1));
        Assert.False(ActorInventoryActions.GiveInventory(target, "SurgeryKit", 2));
        Assert.False(ActorInventoryActions.GiveInventory(player, null));
        Assert.False(ActorInventoryActions.GiveInventory(player, "Unknown"));
    }

    [Fact]
    public void FamilyCountsSuccessfulOwnersAndSiblingsExcludeCaller()
    {
        var sim = Room(); var parent = sim.AddBot(20, 0); var players = sim.Players.ToArray();
        players[0].MasterId = parent.Id; players[1].MasterId = parent.Id;
        var monster = sim.AddBot(40, 0); monster.MasterId = parent.Id;
        players[0].Inventory.GiveHealthPickup("MedPatch", 20);
        Assert.Equal(1, ActorInventoryActions.GiveToChildren(parent, "MedPatch", 3));
        Assert.Equal(20, players[0].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(3, players[1].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(0, players[2].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(1, ActorInventoryActions.GiveToSiblings(players[0], "MedPatch", 2));
        Assert.Equal(5, players[1].Inventory.CountHealthPickup("MedPatch"));
        Assert.Equal(0, ActorInventoryActions.GiveToSiblings(parent, "MedPatch"));
        players[1].Destroy(); Assert.Equal(0, ActorInventoryActions.GiveToSiblings(players[0], "MedPatch"));
    }

    [Fact]
    public void ExistingUnlimitedStackAndRavenGameLimitUseSharedPickupRules()
    {
        var sim = Room(unlimited: true); var player = sim.Players.First();
        Assert.True(ActorInventoryActions.GiveInventory(player, "ArtiHealth", 100));
        Assert.Equal(16, player.Inventory.CountHealthPickup("ArtiHealth"));
        Assert.True(ActorInventoryActions.GiveInventory(player, "ArtiHealth", 100));
        Assert.Equal(116, player.Inventory.CountHealthPickup("ArtiHealth"));
        var saved = SimSavegame.Write(sim); player.Inventory.ResetToPistolStart(); SimSavegame.Apply(sim, saved);
        Assert.Equal(116, player.Inventory.CountHealthPickup("ArtiHealth"));
        player.Destroy(); Assert.False(ActorInventoryActions.GiveInventory(player, "ArtiHealth"));
    }

    private static AuthoritySimulation Room(bool unlimited = false) => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative, UnlimitedHealthPickups: unlimited, DefaultInventoryMaximum: 16));
}
