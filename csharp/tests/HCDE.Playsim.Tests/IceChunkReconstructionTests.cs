using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceChunkReconstructionTests
{
    [Fact]
    public void StartedChunksContinueWithoutReplayingSpawnEventsOrActions()
    {
        var source = Spawn(); source.Tick(); var saved = SimSavegame.Write(source);
        var restored = Room(); var events = 0;
        restored.WorldThingSpawned += actor => { if (actor is IceChunkActor) events++; };
        SimSavegame.Apply(restored, saved);
        Assert.All(restored.Actors.OfType<IceChunkActor>(), chunk => Assert.False(chunk.JustSpawned));
        Assert.Equal(saved, SimSavegame.Write(restored));
        source.Tick(); restored.Tick();
        Assert.Equal(0, events); Assert.Equal(source.Checksum, restored.Checksum);
        Assert.Equal(SimSavegame.Write(source), SimSavegame.Write(restored));
    }

    [Fact]
    public void EmptyChunkArchiveRemovesExistingChunksAndPreservesNextIdentitySafety()
    {
        var source = Spawn(); var populated = SimSavegame.Write(source);
        var maximumId = source.Actors.Max(actor => actor.Id);
        foreach (var chunk in source.Actors.OfType<IceChunkActor>()) chunk.Destroy();
        source.Tick(); var empty = SimSavegame.Write(source);
        var restored = Room(); SimSavegame.Apply(restored, populated);
        SimSavegame.Apply(restored, empty);
        Assert.Empty(restored.Actors.OfType<IceChunkActor>());
        restored.SpawnIceChunks(Corpse(restored));
        Assert.All(restored.Actors.OfType<IceChunkActor>(), chunk => Assert.True(chunk.Id > maximumId));
    }

    [Fact]
    public void ChunkMarkerCannotReplaceExistingPlayerIdentity()
    {
        var sim = Spawn(); var state = sim.CaptureState();
        state.Actors[0].IceChunkLifecycle = 1;
        var checksum = sim.Checksum;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(checksum, sim.Checksum); Assert.Single(sim.Players);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void MalformedLifecycleArchiveFailsValidation(int mutation)
    {
        var state = Spawn().CaptureState(); state.FreezeChunksRandomState = null;
        var saved = SimSavegame.Write(state);
        var size = BinaryPrimitives.ReadInt32LittleEndian(saved.AsSpan(saved.Length - 4));
        var start = saved.Length - size;
        if (mutation == 0) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 4), 11);
        else if (mutation == 1) BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start), 114);
        else BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(start + 20), 2);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(mutation == 0 ? "save-icechunk-size" : mutation == 1 ? "save-icechunk-header" : "save-icechunk-value", error);
    }

    private static Actor Corpse(AuthoritySimulation sim) => new()
    { Simulation = sim, Level = sim.Level, Radius = Fixed.FromInt(20), Height = Fixed.FromInt(64) };
    private static AuthoritySimulation Spawn() { var sim = Room(); sim.SpawnIceChunks(Corpse(sim)); return sim; }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }] }, rngSeed: 42);
}
