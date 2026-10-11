using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorAmmoSetTests
{
    [Theory]
    [InlineData("Clip", 200)]
    [InlineData("Shell", 50)]
    [InlineData("RocketAmmo", 50)]
    [InlineData("Cell", 300)]
    public void SetterClampsIncreasesAndAllowsOverMaximumDecreases(string name, int maximum)
    {
        var player = Room().Players.Single();
        Assert.True(ActorInventoryActions.SetInventory(player, name.ToLowerInvariant(), maximum + 100));
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, name, false));
        Assert.False(ActorInventoryActions.SetInventory(player, name, maximum));
        Assert.True(ActorInventoryActions.SetInventory(player, name, maximum + 100));
        Assert.Equal(maximum, AcsPlayerInventory.Count(player, name, false));
        Assert.True(ActorInventoryActions.SetInventory(player, name, int.MaxValue, beyondMax: true));
        Assert.True(ActorInventoryActions.SetInventory(player, name, maximum + 50));
        Assert.Equal(maximum + 50, AcsPlayerInventory.Count(player, name, false));
        Assert.True(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.False(ActorInventoryActions.SetInventory(player, name, 0));
        Assert.False(ActorInventoryActions.SetInventory(player, name, -1));
        Assert.Equal(maximum, AcsPlayerInventory.AmmoCapacity(player, name));
    }

    [Fact]
    public void SetterUsesChangedCapacityAndPreservesUnrelatedPoolsAndWeapons()
    {
        var player = Room().Players.Single(); player.Inventory.MaxBullets = 400;
        player.Inventory.Shells = 12;
        Assert.True(ActorInventoryActions.SetInventory(player, "Clip", 500));
        Assert.Equal(400, player.Inventory.Bullets); Assert.Equal(12, player.Inventory.Shells);
        Assert.True(player.Inventory.Owns(WeaponKind.Pistol));
        Assert.True(ActorInventoryActions.SetInventory(player, "Clip", int.MinValue));
        Assert.Equal(0, player.Inventory.Bullets); Assert.Equal(400, player.Inventory.MaxBullets);
    }

    [Fact]
    public void StateCallbackResolvesMasterAndSetterSurvivesSave()
    {
        var sim = Room(); var player = sim.Players.Single(); var caller = sim.AddBot(20, 0);
        caller.MasterId = player.Id; var result = false;
        caller.States.Configure(caller, [new(-1, 0), new(5, 0,
            Action: self => result = ActorInventoryActions.SetInventory(self, "Cell", 700, 4, true))], 0);
        caller.States.Enter(caller, 1); Assert.True(result);
        var saved = SimSavegame.Write(sim); player.Inventory.Cells = 0; SimSavegame.Apply(sim, saved);
        Assert.Equal(700, player.Inventory.Cells); Assert.Equal(300, player.Inventory.MaxCells);
        Assert.False(ActorInventoryActions.SetInventory(player, "Cell", 20, pointerSelector: 1));
        Assert.False(ActorInventoryActions.SetInventory(caller, "Cell", 20));
        Assert.False(ActorInventoryActions.SetInventory(player, "CellPack", 20));
        player.Destroy(); Assert.False(ActorInventoryActions.SetInventory(player, "Cell", 20));
        Assert.Equal(700, player.Inventory.Cells);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
