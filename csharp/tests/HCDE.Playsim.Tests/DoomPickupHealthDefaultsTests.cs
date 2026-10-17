using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DoomPickupHealthDefaultsTests
{
    [Theory]
    [InlineData(2011)]
    [InlineData(2012)]
    [InlineData(2014)]
    [InlineData(2015)]
    [InlineData(2018)]
    [InlineData(2019)]
    [InlineData(2013)]
    [InlineData(83)]
    [InlineData(2023)]
    [InlineData(2025)]
    [InlineData(5)]
    [InlineData(2007)]
    [InlineData(2048)]
    [InlineData(2010)]
    [InlineData(2046)]
    [InlineData(2047)]
    [InlineData(17)]
    [InlineData(2008)]
    [InlineData(2049)]
    [InlineData(8)]
    [InlineData(2006)]
    [InlineData(2002)]
    [InlineData(2005)]
    [InlineData(2003)]
    [InlineData(2004)]
    [InlineData(2001)]
    [InlineData(82)]
    [InlineData(5010)]
    public void StockMapAndDroppedInventoryInheritActorHealthAndReconstruct(ushort editor)
    {
        var sim = Room(editor); var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.Equal(1000, map.Health); Assert.Equal(1000, map.ResurrectionHealth); Assert.False(map.CanTakeDamage);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        Assert.Equal(1000, drop.Health); Assert.Equal(1000, drop.ResurrectionHealth); Assert.False(drop.CanTakeDamage);
        var saved = SimSavegame.Write(sim, true); map.Destroy(); drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(editor); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData("Mass = 240", 1000)]
    [InlineData("Hit points = 75", 75)]
    public void UnrelatedAndExplicitPatchesRespectHealthDefaults(string body, int expected)
    {
        var patch = DehackedPatch.Apply($"Thing 64\n{body}\n"); Assert.Empty(patch.Errors);
        var sim = Room(2007, patch); Assert.Equal(expected, sim.Actors.Single(a => a.MapPickupThingIndex.HasValue).Health);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), PickupCatalog.Clip));
        Assert.Equal(expected, sim.Actors.Single(a => a.SpawnedDroppedPickup).Health);
    }

    private static AuthoritySimulation Room(ushort editor, DehackedPatchResult? patch = null) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 400 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch, compat: CompatSurface.NoTossDrops);
}
