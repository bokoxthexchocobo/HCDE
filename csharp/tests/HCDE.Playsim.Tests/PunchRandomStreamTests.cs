using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class PunchRandomStreamTests
{
    [Theory]
    [InlineData(0, 6, -3.779296875, 17390348842389354777UL)]
    [InlineData(42, 10, -0.46142578125, 17560395492416470006UL)]
    public void PunchMatchesNamedPcgReference(int seed, int damage, double spread, ulong state)
    {
        var sim = Room(seed); Assert.Equal(damage, sim.NextPunchDamage());
        Assert.Equal(spread, sim.NextPunchSpread()); Assert.Equal(state, sim.CaptureState().PunchRandomState);
    }

    [Fact]
    public void CombatAndSawDrawsDoNotChangePunchSequence()
    {
        var sim = Room(); var control = Room();
        for (var i = 0; i < 30; i++) { sim.NextCombatRandom(); sim.NextSawByte(); }
        Assert.Equal(control.NextPunchDamage(), sim.NextPunchDamage());
        Assert.Equal(control.NextPunchSpread(), sim.NextPunchSpread());
    }

    [Fact]
    public void FistFiringUsesPunchStreamWithoutAdvancingOtherStreams()
    {
        var sim = Room(); var control = Room(); var player = sim.Players.Single();
        player.Inventory.Selected = WeaponKind.Fist; Assert.True(HitscanCombat.Fire(sim, player));
        Assert.Equal(control.NextCombatRandom(), sim.NextCombatRandom());
        Assert.Equal(control.NextSawByte(), sim.NextSawByte()); Assert.NotNull(sim.CaptureState().PunchRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PunchArchiveContinuesAndRejectsMalformedTrailer(bool header)
    {
        var sim = Room(); sim.NextPunchDamage(); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(250, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var next = sim.NextPunchSpread(); SimSavegame.Apply(sim, saved);
        Assert.Equal(saved, SimSavegame.Write(sim, true)); Assert.Equal(next, sim.NextPunchSpread());
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - (header ? 16 : 4)), header ? 250 : 12);
        Assert.False(SimSavegame.TryRead(saved, out _, out _));
    }

    [Fact]
    public void OlderSnapshotResetsPunchStream()
    {
        var sim = Room(); var saved = SimSavegame.Write(sim, true); var first = sim.NextPunchDamage();
        sim.NextPunchSpread(); SimSavegame.Apply(sim, saved);
        Assert.Null(sim.CaptureState().PunchRandomState); Assert.Equal(first, sim.NextPunchDamage());
    }

    private static AuthoritySimulation Room(int seed = 42) => AuthoritySimulation.Start(new PlayLevel {
        Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }]
    }, rngSeed: seed);
}
