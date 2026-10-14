using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SawRandomStreamTests
{
    [Theory]
    [InlineData(0, new[] { 51, 9, 8, 55, 151, 90 }, 12634178173439515009UL)]
    [InlineData(42, new[] { 89, 182, 129, 231, 254, 166 }, 6810635572257267675UL)]
    public void SawBytesMatchNamedPcgReference(int seed, int[] expected, ulong finalState)
    {
        var sim = Room(seed);
        Assert.Equal(expected, Enumerable.Range(0, 6).Select(_ => sim.NextSawByte()));
        Assert.Equal(finalState, sim.CaptureState().SawRandomState);
    }

    [Fact]
    public void SharedCombatDrawsDoNotChangeSawSequence()
    {
        var sim = Room(); var control = Room();
        for (var i = 0; i < 30; i++) sim.NextCombatRandom();
        Assert.Equal(control.NextSawDamageMultiplier(), sim.NextSawDamageMultiplier());
        Assert.Equal(control.NextSawSpread(), sim.NextSawSpread());
    }

    [Fact]
    public void ChainsawAttackDoesNotAdvanceSharedCombatStream()
    {
        var sim = Room(); var control = Room();
        ChainsawActions.Attack(sim.Players.Single(), flags: ChainsawAttackFlags.RandomLightBoth);
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        Assert.NotNull(sim.CaptureState().SawRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StreamRoundTripsAndMalformedTrailerRejects(bool header)
    {
        var sim = Room(); sim.NextSawByte(); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(249, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var next = sim.NextSawByte(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.Equal(next, sim.NextSawByte());
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - (header ? 16 : 4)), header ? 249 : 12);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void OlderSnapshotResetsSawStreamToInitialSeed()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); var expected = sim.NextSawByte();
        sim.NextSawByte(); SimSavegame.Apply(sim, saved);
        Assert.Null(sim.CaptureState().SawRandomState); Assert.Equal(expected, sim.NextSawByte());
    }

    private static AuthoritySimulation Room(int seed = 42) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    }, rngSeed: seed);
}
