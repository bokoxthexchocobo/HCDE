using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgSelfDamageRandomTests
{
    [Fact]
    public void CombatDrawsDoNotChangeBfgDamageAndBfgDoesNotAdvanceCombat()
    {
        var sim = Room(); var control = Room();
        for (var i = 0; i < 31; i++) sim.NextCombatRandom();
        var combat = sim.CombatRandomState;
        for (var i = 0; i < 4; i++) Assert.Equal(Roll(control), Roll(sim));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(control.CaptureState().BfgSelfDamageRandomState, sim.CaptureState().BfgSelfDamageRandomState);
    }

    [Fact]
    public void FreshSaveContinuesSequenceAndLegacySaveResetsIt()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim, true);
        var first = Roll(sim); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(357, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 4; i++) Assert.Equal(Roll(sim), Roll(fresh));
        SimSavegame.Apply(fresh, legacy);
        Assert.Null(fresh.CaptureState().BfgSelfDamageRandomState);
        Assert.Equal(first, Roll(fresh));
    }

    [Theory]
    [InlineData(0, 50, 10)]
    [InlineData(1, 100, 10)]
    [InlineData(1, 50, 0)]
    public void NonRandomOrOutsideRadiusDoesNotConsumeStream(int flags, int x, int damage)
    {
        var sim = Room(); sim.Players.Single().X = Fixed.FromInt(x);
        ActorRadiusSelfActions.Apply(Source(sim), damage, 100, flags);
        Assert.Null(sim.CaptureState().BfgSelfDamageRandomState);
    }

    [Fact]
    public void InvalidArchivePredecessorIsRejected()
    {
        var sim = Room(); Roll(sim); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 16), 357);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-bfgselfdamagerandom-header", error);
    }

    [Fact]
    public void MissingGeometryRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.BfgSelfDamageRandomState = 42; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void StreamParticipatesInChecksumWithoutDamage()
    {
        var sim = Room(); var control = Room(); sim.NextBfgSelfDamageRandom();
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
    }

    private static int Roll(AuthoritySimulation sim)
    {
        var target = sim.Players.Single(); var before = target.Health;
        ActorRadiusSelfActions.Apply(Source(sim), 10, 100, 1);
        var damage = before - target.Health; Assert.InRange(damage, 5, 40); return damage;
    }

    private static Actor Source(AuthoritySimulation sim) => sim.Actors.Single(a => a is not PlayerPawn);
    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1, X = 50 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        }, rngSeed: 1);
        var target = sim.Players.Single(); target.ThingId = 7; target.Health = 1000;
        sim.AddBot(0, 0).Brain!.SetTargetThingId(sim, 7);
        return sim;
    }
}
