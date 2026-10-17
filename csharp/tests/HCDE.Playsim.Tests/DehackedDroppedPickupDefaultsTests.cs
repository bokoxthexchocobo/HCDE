using HCDE.Gamedata;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DehackedDroppedPickupDefaultsTests
{
    [Theory]
    [InlineData(54, 2011)]
    [InlineData(64, 2007)]
    [InlineData(72, 8)]
    [InlineData(73, 2006)]
    [InlineData(78, 2001)]
    public void DroppedClassDimensionsHealthAndMassMatchMapDefaults(int index, int editor)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nWidth = 786432\nHeight = 2097152\nHit points = 75\nMass = 240\n");
        Assert.Empty(patch.Errors);
        var sim = Room((ushort)editor, patch);
        var map = sim.Actors.Single(a => a.MapPickupThingIndex.HasValue);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        Assert.Equal(12, drop.Radius.ToDouble()); Assert.Equal(32, drop.Height.ToDouble());
        Assert.Equal(75, drop.Health); Assert.Equal(240, drop.Mass);
        Assert.Equal(map.ResurrectionHealth, drop.ResurrectionHealth);
        Assert.Equal(map.Radius, drop.Radius); Assert.Equal(map.Height, drop.Height);
        Assert.True(drop.Dropped); Assert.False(drop.Solid); Assert.False(drop.Shootable);
        var bytes = SimSavegame.Write(sim, true);
        drop.Destroy(); sim.Tick(); SimSavegame.Apply(sim, bytes);
        Assert.Equal(bytes, SimSavegame.Write(sim, true));
        var fresh = Room((ushort)editor, patch); SimSavegame.Apply(fresh, bytes);
        Assert.Equal(bytes, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 5; i++) { sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum); }
    }

    [Theory]
    [InlineData(72, 8, 26)]
    [InlineData(73, 2006, 20)]
    [InlineData(78, 2001, 16)]
    public void UnrelatedPatchPreservesInheritedDimensions(int index, int editor, int height)
    {
        var patch = DehackedPatch.Apply($"Thing {index}\nMass = 240\n");
        var sim = Room((ushort)editor, patch);
        Assert.True(sim.SpawnDroppedPickup(sim.Players.Single(), editor));
        var drop = sim.Actors.Single(a => a.SpawnedDroppedPickup);
        Assert.Equal(20, drop.Radius.ToDouble()); Assert.Equal(height, drop.Height.ToDouble());
        Assert.Equal(1000, drop.Health); Assert.Equal(240, drop.Mass);
    }

    private static AuthoritySimulation Room(ushort editor, DehackedPatchResult patch) => AuthoritySimulation.Start(
        new PlayLevel { Things = [new LevelThing { Type = 1 }, new LevelThing { Type = editor, X = 200 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] }, dehacked: patch);
}
