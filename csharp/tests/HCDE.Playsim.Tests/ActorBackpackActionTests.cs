using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorBackpackActionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void GrantCountDoesNotMultiplyAmmoAndDuplicatesRemainSuccessful(int amount)
    {
        var player = Room().Players.First(); var inventory = player.Inventory;
        Assert.True(ActorInventoryActions.GiveInventory(player, "bAcKpAcK", amount));
        Assert.True(inventory.HasBackpack);
        Assert.Equal((60, 4, 1, 20), (inventory.Bullets, inventory.Shells, inventory.Rockets, inventory.Cells));
        Assert.Equal((400, 100, 100, 600), (inventory.MaxBullets, inventory.MaxShells, inventory.MaxRockets, inventory.MaxCells));
        inventory.Bullets = 400; inventory.Shells = 100; inventory.Rockets = 100; inventory.Cells = 600;
        Assert.True(ActorInventoryActions.GiveInventory(player, "Backpack"));
        Assert.Equal(400, inventory.Bullets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    public void RemovalResetsOnlyBackpackCapacitiesAndClampsExcessAmmo(int amount)
    {
        var player = Room().Players.First(); var inventory = player.Inventory; inventory.GiveBackpack();
        inventory.MaxShells = 125; inventory.Shells = 120;
        inventory.Bullets = 350; inventory.Rockets = 90; inventory.Cells = 550;
        Assert.True(ActorInventoryActions.TakeInventory(player, "Backpack", amount, flags: 1));
        Assert.False(inventory.HasBackpack);
        Assert.Equal((200, 125, 50, 300), (inventory.MaxBullets, inventory.MaxShells, inventory.MaxRockets, inventory.MaxCells));
        Assert.Equal((200, 120, 50, 300), (inventory.Bullets, inventory.Shells, inventory.Rockets, inventory.Cells));
        Assert.False(ActorInventoryActions.TakeInventory(player, "Backpack"));
    }

    [Fact]
    public void TargetStateCallbackAndFamilyActionsUseSharedSelection()
    {
        var sim = Room(); var players = sim.Players.ToArray(); var parent = sim.AddBot(20, 0);
        players[0].MasterId = players[1].MasterId = parent.Id; parent.Brain!.SetSpecialTarget(players[0]);
        var result = false;
        parent.States.Configure(parent, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.GiveToTarget(self, "Backpack"))], 0);
        parent.States.Enter(parent, 1); Assert.True(result);
        Assert.Equal(1, ActorInventoryActions.GiveToSiblings(players[0], "Backpack"));
        Assert.Equal(2, ActorInventoryActions.TakeFromChildren(parent, "Backpack"));
        Assert.False(players[2].Inventory.HasBackpack);
        Assert.Equal(0, ActorInventoryActions.TakeFromSiblings(players[0], "Backpack"));
        Assert.False(ActorInventoryActions.GiveInventory(players[0], "Backpack", pointerSelector: 1));
        Assert.False(ActorInventoryActions.GiveInventory(parent, "Backpack"));
    }

    [Fact]
    public void RemovalPersistsAfterSaveAndDestroyedReceiversPreservePack()
    {
        var sim = Room(); var player = sim.Players.First(); player.Inventory.GiveBackpack();
        Assert.True(ActorInventoryActions.TakeInventory(player, "Backpack"));
        var saved = SimSavegame.Write(sim); player.Inventory.GiveBackpack(); SimSavegame.Apply(sim, saved);
        Assert.False(player.Inventory.HasBackpack); Assert.Equal(200, player.Inventory.MaxBullets);
        player.Inventory.GiveBackpack(); player.Destroy();
        Assert.False(ActorInventoryActions.TakeInventory(player, "Backpack"));
        Assert.False(ActorInventoryActions.GiveInventory(player, "Backpack")); Assert.True(player.Inventory.HasBackpack);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 2, X = 300 }, new LevelThing { Type = 3, X = 600 }]
    }, spawnOptions: new SpawnOptions(Mode: SpawnGameMode.Cooperative));
}
