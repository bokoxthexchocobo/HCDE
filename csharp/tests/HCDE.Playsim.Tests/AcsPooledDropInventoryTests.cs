using HCDE.MapLoader;
namespace HCDE.Playsim.Tests;

public class AcsPooledDropInventoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DynamicNameDropsSameItemAsModuleName(bool pooled)
    {
        var sim = Room(); var player = sim.Players.Single(); player.Inventory.Bullets = 80;
        var strings = new AcsGlobalStrings(); var id = pooled ? strings.Add("cLiP") : 0;
        Drop(sim, player, 0, id, ["Clip"], strings);
        Assert.Equal(79, player.Inventory.Bullets);
        var item = Assert.Single(sim.Actors, a => a.SpawnedDroppedPickup);
        Assert.Equal(PickupCatalog.Clip, item.DoomEdNum); Assert.Equal(30, item.PickupDelay);
    }

    [Fact]
    public void TidLookupFindsInventoryWithoutActivator()
    {
        var sim = Room(); var player = sim.Players.Single(); player.ThingId = 9; var strings = new AcsGlobalStrings();
        var before = player.Inventory.Bullets; Drop(sim, null, 9, strings.Add("Clip"), [], strings);
        Assert.Equal(before - 1, player.Inventory.Bullets); Assert.Single(sim.Actors, a => a.SpawnedDroppedPickup);
    }

    [Theory]
    [InlineData("PowerIronFeet")]
    [InlineData("UnknownInventory")]
    [InlineData("")]
    public void NonDroppableOrUnknownDynamicNamesLeaveInventoryIntact(string name)
    {
        var sim = Room(); var player = sim.Players.Single(); player.GivePowerIronFeet(); var strings = new AcsGlobalStrings();
        Drop(sim, player, 0, strings.Add(name), [], strings);
        Assert.Equal(2100, player.PowerIronFeetTics); Assert.DoesNotContain(sim.Actors, a => a.SpawnedDroppedPickup);
    }

    [Fact]
    public void PooledDropCanReplayFromSavedState()
    {
        var sim = Room(); var player = sim.Players.Single(); var strings = new AcsGlobalStrings(); var id = strings.Add("Clip");
        var saved = SimSavegame.Write(sim, true); Drop(sim, player, 0, id, [], strings); var expected = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved); Drop(sim, player, 0, id, [], strings); Assert.Equal(expected, SimSavegame.Write(sim, true));
    }

    private static void Drop(AuthoritySimulation sim, Actor? actor, int tid, int id, string[] module, AcsGlobalStrings strings)
    {
        var stack = new List<int> { 77, tid, id };
        Assert.True(AcsCallFunctions.TryInvoke(sim, stack, new() { Value = actor }, module, strings,
            AcsCallFunctions.DropInventory, 2, out var result)); Assert.Equal(0, result); Assert.Equal(new[] { 77 }, stack);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
