using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkInheritedSequencesTests
{
    [Theory]
    [InlineData("Null")]
    [InlineData("GenericCrush")]
    [InlineData("DieFromSpawn")]
    public void InheritedLabelTimingAndFreshReconstructionMatchNative(string label)
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level,
            X = Fixed.FromInt(200), Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) });
        var chunk = sim.Actors.OfType<IceChunkActor>().First();
        Assert.True(AcsActorStates.TryFindNamedState(chunk, label, true, out var state));
        Assert.False(AcsActorStates.HasNamedState(chunk, label + ".More", true));
        Assert.True(AcsActorStates.HasNamedState(chunk, label + ".More", false));
        chunk.States.Enter(chunk, state); Assert.False(chunk.FullBright);
        Assert.Equal(label == "GenericCrush" ? -1 : 1, chunk.States.RemainingTics);
        var saved = SimSavegame.Write(sim, true); chunk.Destroy(); sim.Tick(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true));
        var fresh = Room(); SimSavegame.Apply(fresh, saved); Assert.Equal(saved, SimSavegame.Write(fresh, true));
        var restored = sim.Actors.Single(a => a.Id == chunk.Id); var loaded = fresh.Actors.Single(a => a.Id == chunk.Id);
        sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
        if (label == "GenericCrush")
        {
            Assert.False(restored.Destroyed); Assert.False(loaded.Destroyed);
            restored.States.ForceRemainingTics(1); loaded.States.ForceRemainingTics(1);
            sim.Tick(); fresh.Tick(); Assert.Equal(sim.Checksum, fresh.Checksum);
        }
        Assert.True(restored.Destroyed); Assert.True(loaded.Destroyed);
    }

    [Fact]
    public void StateOutsideExpandedClassTableIsRejectedBeforeMutation()
    {
        var sim = Room(); sim.SpawnIceChunks(new Actor { Simulation = sim, Level = sim.Level });
        var chunk = sim.Actors.OfType<IceChunkActor>().First();
        chunk.States.Configure(chunk, Enumerable.Repeat(new ActorFrame(-1, 10), 11), 10);
        var state = sim.CaptureState();
        var before = SimSavegame.Write(sim, true);
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel {
        Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 256 }],
    });
}
