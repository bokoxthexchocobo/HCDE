using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BfgSprayRandomTests
{
    [Fact]
    public void CombatAndSelfDamageDrawsDoNotChangeSprayDamage()
    {
        var sim = Room(); var control = Room();
        for (var i = 0; i < 31; i++) { sim.NextCombatRandom(); sim.NextBfgSelfDamageRandom(); }
        var combat = sim.CombatRandomState; var self = sim.CaptureState().BfgSelfDamageRandomState;
        for (var i = 0; i < 4; i++) Assert.Equal(Roll(control), Roll(sim));
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Equal(self, sim.CaptureState().BfgSelfDamageRandomState);
        Assert.Equal(control.CaptureState().BfgSprayRandomState, sim.CaptureState().BfgSprayRandomState);
    }

    [Fact]
    public void FreshSaveContinuesSprayAndLegacySaveResetsStream()
    {
        var sim = Room(); var legacy = SimSavegame.Write(sim, true);
        var first = Roll(sim); var saved = SimSavegame.Write(sim, true);
        Assert.Equal(358, BinaryPrimitives.ReadUInt16LittleEndian(saved.AsSpan(4)));
        var fresh = Room(); SimSavegame.Apply(fresh, saved);
        Assert.Equal(saved, SimSavegame.Write(fresh, true));
        for (var i = 0; i < 4; i++) Assert.Equal(Roll(sim), Roll(fresh));
        SimSavegame.Apply(fresh, legacy); Assert.Null(fresh.CaptureState().BfgSprayRandomState);
        Assert.Equal(first, Roll(fresh));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FixedDamageMissingOwnerAndMissesLeaveStreamUnused(int route)
    {
        var sim = Room(); var owner = sim.Players.Single();
        BfgSprayActions.Apply(sim, Missile(), route == 1 ? null : owner, numRays: 1,
            distance: route == 2 ? 50 : 1024, fixedDamage: route == 0 ? 25 : 0);
        Assert.Null(sim.CaptureState().BfgSprayRandomState);
    }

    [Fact]
    public void InvalidArchivePredecessorIsRejected()
    {
        var sim = Room(); Roll(sim); var saved = SimSavegame.Write(sim, true);
        BinaryPrimitives.WriteInt32LittleEndian(saved.AsSpan(saved.Length - 16), 358);
        Assert.False(SimSavegame.TryRead(saved, out _, out var error));
        Assert.Equal("save-bfgsprayrandom-header", error);
    }

    [Fact]
    public void MissingGeometryRejectsWriteAndRestoreBeforeMutation()
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true); var state = sim.CaptureState();
        state.BfgSprayRandomState = 42; state.GeometryHealth = null;
        Assert.Throws<InvalidOperationException>(() => SimSavegame.Write(state));
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void StreamParticipatesInChecksumWithoutDamage()
    {
        var sim = Room(); var control = Room(); sim.NextBfgSprayDamageDie();
        sim.Tick(); control.Tick(); Assert.NotEqual(control.Checksum, sim.Checksum);
    }

    private static int Roll(AuthoritySimulation sim)
    {
        var target = sim.Actors.Single(a => a is not PlayerPawn); var before = target.Health;
        BfgSprayActions.Apply(sim, Missile(), sim.Players.Single(), numRays: 1, damageCount: 5);
        var damage = before - target.Health; Assert.InRange(damage, 5, 40); return damage;
    }

    private static Actor Missile() => new() { Angle = BamAngle.FromDegrees(45) };
    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 512 }],
        }, rngSeed: 42);
        var target = sim.AddBot(200, 0, 3001); target.Brain = null;
        target.Health = 1000; target.PainChance = 0;
        return sim;
    }
}
