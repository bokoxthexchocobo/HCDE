using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class UniqueTidRandomTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    public void DrawsMatchNamedNativeStream(int seed)
    {
        var sim = Room(seed); var state = NativeStateRandom.Seed(unchecked((uint)seed), 0xc388ea6bu);
        Assert.Null(sim.UniqueTidRandomState);
        var legacy = sim.SwitchTargetRandomState;
        for (var i = 0; i < 256; i++)
        {
            Assert.Equal(NativeStateRandom.Next(ref state), sim.NextUniqueTidRandom());
            Assert.Equal(state, sim.UniqueTidRandomState);
        }
        Assert.Equal(legacy, sim.SwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedStreamContinuesExactlyAndOlderSaveClearsIt()
    {
        var sim = Room(); var older = SimSavegame.Write(sim, true);
        for (var i = 0; i < 17; i++) sim.NextUniqueTidRandom();
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(300, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var expected = Enumerable.Range(0, 64).Select(_ => sim.NextUniqueTidRandom()).ToArray();
        var final = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, Enumerable.Range(0, 64).Select(_ => sim.NextUniqueTidRandom()).ToArray());
        Assert.Equal(final, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.Null(sim.UniqueTidRandomState);
        Assert.Equal(Room().NextUniqueTidRandom(), sim.NextUniqueTidRandom());
    }

    [Fact]
    public void LegacyAndNativeStreamsPersistTogether()
    {
        var sim = Room(); sim.NextSwitchTargetRandom(); sim.NextUniqueTidRandom();
        var saved = SimSavegame.Write(sim, true);
        var legacy = sim.NextSwitchTargetRandom(); var native = sim.NextUniqueTidRandom();
        SimSavegame.Apply(sim, saved);
        Assert.Equal(legacy, sim.NextSwitchTargetRandom()); Assert.Equal(native, sim.NextUniqueTidRandom());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedTrailerRejects(bool header)
    {
        var sim = Room(); sim.NextUniqueTidRandom(); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - (header ? 16 : 4)), header ? 300 : 15);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-uniquetidrandom-header" : "save-uniquetidrandom-size", error);
    }

    [Fact]
    public void MissingGeometryRejectsBeforeMutation()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.GeometryHealth = null; state.UniqueTidRandomState = 0;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Null(sim.UniqueTidRandomState);
    }

    [Fact]
    public void ZeroStateIsNotTreatedAsAbsent()
    {
        var sim = Room(); var state = sim.CaptureState(); state.UniqueTidRandomState = 0;
        var saved = SimSavegame.Write(state); SimSavegame.Apply(sim, saved);
        Assert.Equal(0UL, sim.UniqueTidRandomState);
        ulong expected = 0;
        Assert.Equal(NativeStateRandom.Next(ref expected), sim.NextUniqueTidRandom());
        Assert.Equal(expected, sim.UniqueTidRandomState);
    }

    private static AuthoritySimulation Room(int seed = 42) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
}
