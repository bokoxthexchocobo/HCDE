using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeSwitchTargetRandomTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(42)]
    [InlineData(-1)]
    public void DrawsMatchNamedNativeStream(int seed)
    {
        var sim = Room(seed); var state = NativeStateRandom.Seed(unchecked((uint)seed), 0x71a89a33u);
        Assert.Null(sim.NativeSwitchTargetRandomState);
        var legacy = sim.SwitchTargetRandomState;
        for (var i = 0; i < 256; i++)
        {
            Assert.Equal((int)(NativeStateRandom.Next(ref state) & 255), sim.NextSwitchTargetByte());
            Assert.Equal(state, sim.NativeSwitchTargetRandomState);
        }
        Assert.Equal(legacy, sim.SwitchTargetRandomState);
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void SavedStreamContinuesExactlyAndOlderSaveClearsIt()
    {
        var sim = Room(); var older = SimSavegame.Write(sim, true);
        for (var i = 0; i < 17; i++) sim.NextSwitchTargetByte();
        var saved = SimSavegame.Write(sim, true);
        Assert.Equal(289, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var expected = Enumerable.Range(0, 64).Select(_ => sim.NextSwitchTargetByte()).ToArray();
        var final = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        Assert.Equal(expected, Enumerable.Range(0, 64).Select(_ => sim.NextSwitchTargetByte()).ToArray());
        Assert.Equal(final, SimSavegame.Write(sim, true));
        SimSavegame.Apply(sim, older); Assert.Null(sim.NativeSwitchTargetRandomState);
        Assert.Equal(Room().NextSwitchTargetByte(), sim.NextSwitchTargetByte());
    }

    [Fact]
    public void LegacyAndNativeStreamsPersistTogether()
    {
        var sim = Room(); sim.NextSwitchTargetRandom(); sim.NextSwitchTargetByte();
        var saved = SimSavegame.Write(sim, true);
        var legacy = sim.NextSwitchTargetRandom(); var native = sim.NextSwitchTargetByte();
        SimSavegame.Apply(sim, saved);
        Assert.Equal(legacy, sim.NextSwitchTargetRandom()); Assert.Equal(native, sim.NextSwitchTargetByte());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MalformedTrailerRejects(bool header)
    {
        var sim = Room(); sim.NextSwitchTargetByte(); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - (header ? 16 : 4)), header ? 289 : 15);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal(header ? "save-nativeswitchtargetrandom-header" : "save-nativeswitchtargetrandom-size", error);
    }

    [Fact]
    public void MissingGeometryRejectsBeforeMutation()
    {
        var sim = Room(); var state = sim.CaptureState();
        state.GeometryHealth = null; state.NativeSwitchTargetRandomState = 0;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Null(sim.NativeSwitchTargetRandomState);
    }

    [Fact]
    public void ZeroStateIsNotTreatedAsAbsent()
    {
        var sim = Room(); var state = sim.CaptureState(); state.NativeSwitchTargetRandomState = 0;
        var saved = SimSavegame.Write(state); SimSavegame.Apply(sim, saved);
        Assert.Equal(0UL, sim.NativeSwitchTargetRandomState);
        ulong expected = 0;
        Assert.Equal((int)(NativeStateRandom.Next(ref expected) & 255), sim.NextSwitchTargetByte());
        Assert.Equal(expected, sim.NativeSwitchTargetRandomState);
    }

    private static AuthoritySimulation Room(int seed = 42) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] }, rngSeed: seed);
}
