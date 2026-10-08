using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkCreationTimingTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    public void CreatedChunksDrawOneIceDurationEachWithoutRedundantCombatTiming(int seed)
    {
        var sim = Room(seed); var control = Room(seed);
        var corpse = new Actor
        {
            Simulation = sim, Level = sim.Level, Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64),
        };
        sim.SpawnIceChunks(corpse);
        var chunks = sim.Actors.OfType<IceChunkActor>().ToArray();
        Assert.NotEmpty(chunks);
        foreach (var chunk in chunks) Assert.Equal(control.NextIceTics(), chunk.RemainingTics);
        Assert.Equal(control.NextIceTics(), sim.NextIceTics());
        Assert.Equal(control.CombatRandomState, sim.CombatRandomState);
        Assert.Equal(control.NextStateRandom(), sim.NextStateRandom());
    }

    [Fact]
    public void FreshWorldRecreatesChunkTimersAndIceStreamWithoutReplayingEntry()
    {
        var sim = Room(42);
        sim.SpawnIceChunks(new Actor
        { Simulation = sim, Level = sim.Level, Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var saved = SimSavegame.Write(sim);
        var timers = sim.Actors.OfType<IceChunkActor>().Select(chunk => chunk.RemainingTics).ToArray();
        var next = sim.NextIceTics(); var restored = Room(42);
        SimSavegame.Apply(restored, saved);
        Assert.Equal(timers, restored.Actors.OfType<IceChunkActor>().Select(chunk => chunk.RemainingTics));
        Assert.Equal(saved, SimSavegame.Write(restored));
        Assert.Equal(next, restored.NextIceTics());
    }

    private static AuthoritySimulation Room(int seed) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] }, rngSeed: seed);
}
