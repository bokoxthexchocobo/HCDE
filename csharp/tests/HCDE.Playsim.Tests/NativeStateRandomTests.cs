using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class NativeStateRandomTests
{
    [Theory]
    [InlineData(0u, 1782629141u, 3818651163u, 924375151u, 3537659108u, 8053324316674391245UL)]
    [InlineData(42u, 1594465970u, 1558181585u, 1151275222u, 1249266070u, 4239841205283106507UL)]
    [InlineData(uint.MaxValue, 3340545193u, 2472368391u, 3492205054u, 240301642u, 13433392972162730160UL)]
    public void NativeSeedingAndDrawsMatchFixedReferenceVectors(uint seed, uint first, uint second,
        uint third, uint fourth, ulong finalState)
    {
        var state = NativeStateRandom.Seed(seed);
        Assert.Equal(first, NativeStateRandom.Next(ref state));
        Assert.Equal(second, NativeStateRandom.Next(ref state));
        Assert.Equal(third, NativeStateRandom.Next(ref state));
        Assert.Equal(fourth, NativeStateRandom.Next(ref state));
        Assert.Equal(finalState, state);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FullStateRoundTripsAndMalformedArchiveRejects(bool corruptHeader)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }] }, rngSeed: 42);
        sim.NextStateRandom(); var saved = SimSavegame.Write(sim);
        Assert.Equal(111, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        Assert.True(SimSavegame.TryRead(saved, out var state, out var error), error);
        Assert.True(state.StateRandomState > uint.MaxValue);
        var next = sim.NextStateRandom(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim)); Assert.Equal(next, sim.NextStateRandom());
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - (corruptHeader ? 16 : 4)), corruptHeader ? 111 : 12);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Apply(sim, saved));
    }
}
