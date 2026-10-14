using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class GibFactorTests
{
    [Theory]
    [InlineData(1, -101)]
    [InlineData(0.5, -50)]
    [InlineData(0, 0)]
    [InlineData(1.5, -151)]
    public void SentinelAppliesFactorAndTruncatesBeforeNegating(double factor, int expected)
    {
        var sim = Room(); sim.GibFactor = factor; var actor = sim.Players.Single();
        actor.ResurrectionHealth = 101; actor.GibHealth = int.MinValue;
        Assert.Equal(expected, actor.GetGibHealth()); actor.GibHealth = 37;
        Assert.Equal(-37, actor.GetGibHealth());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void InvalidFactorRejectsWithoutMutation(double value)
    {
        var sim = Room(); Assert.Throws<ArgumentOutOfRangeException>(() => sim.GibFactor = value);
        Assert.Equal(1, sim.GibFactor);
    }

    [Fact]
    public void SaveRestoresFactorAndOlderSaveRestoresDefault()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim); sim.GibFactor = 0.5;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(228, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        sim.GibFactor = 2; SimSavegame.Apply(sim, bytes);
        Assert.Equal(0.5, sim.GibFactor); Assert.Equal(bytes, SimSavegame.Write(sim));
        SimSavegame.Apply(sim, legacy); Assert.Equal(1, sim.GibFactor); Assert.Equal(legacy, SimSavegame.Write(sim));
    }

    [Fact]
    public void FactorParticipatesInChecksum()
    {
        var sim = Room(); var other = Room(); other.GibFactor = 0.5;
        sim.Tick(); other.Tick(); Assert.NotEqual(sim.Checksum, other.Checksum);
    }

    [Fact]
    public void MalformedBinaryFactorIsRejected()
    {
        var sim = Room(); sim.GibFactor = 0.5; var bytes = SimSavegame.Write(sim);
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(bytes.Length - 12), BitConverter.DoubleToInt64Bits(double.NaN));
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-gibfactor-value", error);
    }

    [Fact]
    public void InvalidStructuredFactorRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var state = sim.CaptureState(); state.GibFactor = -1;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state)); Assert.Equal(1, sim.GibFactor);
    }

    [Fact]
    public void HalfFactorChangesSentinelDeathSelection()
    {
        var sim = Room(); sim.GibFactor = 0.5; var actor = sim.Players.Single();
        actor.GibHealth = int.MinValue; actor.ExtremeDeathState = ActorStateMachine.Corpse;
        ActorDamage.Apply(actor, 151); Assert.True(actor.ExtremelyDead);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1 }]
    });
}
