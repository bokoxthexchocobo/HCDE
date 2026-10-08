using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class IceTicsRandomTests
{
    [Fact]
    public void IceTicsMatchesNativeFormulaVectorAndIsolatesOtherStreams()
    {
        var sim = Room(); var control = Room();
        foreach (var expected in new[] { 98, 91, 107, 105 }) Assert.Equal(expected, sim.NextIceTics());
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        Assert.Equal(control.NextStateRandom(), sim.NextStateRandom());
        Assert.Equal(control.NextMapSpawnRandom(), sim.NextMapSpawnRandom());
    }

    [Fact]
    public void ChunkSuccessorsConsumeSharedIceStreamWithoutCombatDraws()
    {
        var sim = Room(); var control = Room();
        var left = new IceChunkActor(1) { Simulation = sim };
        var right = new IceChunkActor(1) { Simulation = sim };
        for (var frame = 1; frame < 4; frame++)
        {
            left.States.Enter(left, frame); Assert.Equal(control.NextIceTics(), left.RemainingTics);
            right.States.Enter(right, frame); Assert.Equal(control.NextIceTics(), right.RemainingTics);
        }
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
    }

    [Fact]
    public void SavePreservesFullStreamContinuationAndOlderSaveReseeds()
    {
        var sim = Room(); var older = SimSavegame.Write(sim);
        sim.NextIceTics(); sim.NextIceTics(); var saved = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(saved, out var state, out var error), error);
        Assert.True(state.IceTicsRandomState > uint.MaxValue);
        var next = sim.NextIceTics(); var restored = Room(); SimSavegame.Apply(restored, saved);
        Assert.Equal(saved, SimSavegame.Write(restored)); Assert.Equal(next, restored.NextIceTics());
        SimSavegame.Apply(sim, older); Assert.Equal(Room().NextIceTics(), sim.NextIceTics());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedArchiveIsRejectedBeforeApplication(bool header)
    {
        var sim = Room(); sim.NextIceTics(); var saved = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(header ? saved.Length - 16 : saved.Length - 4), header ? 113 : 15);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-icetics-header" : "save-icetics-size", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
}
