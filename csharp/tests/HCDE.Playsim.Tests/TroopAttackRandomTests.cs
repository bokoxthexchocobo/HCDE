using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TroopAttackRandomTests
{
    [Theory]
    [InlineData(3001, false)]
    [InlineData(3001, true)]
    public void ImpConsumesNamedStreamOnlyForInRangeHits(int type, bool leavesRange)
    {
        var sim = Room(type); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        var player = sim.Players.Single(); monster.ReactionTime = 0;
        ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        if (leavesRange) player.X = Fixed.FromInt(500);
        for (var i = 0; i < 16; i++) ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        Assert.Equal(!leavesRange, sim.CaptureState().TroopAttackRandomState.HasValue);
        Assert.Equal(!leavesRange, player.Health < 100);
        Assert.Equal(leavesRange ? 0 : 1, sim.DrainSoundRequests().Count(r => r.Sound == "imp/melee"));
        Assert.Null(sim.CaptureState().SkelFistRandomState);
    }

    [Fact]
    public void DistantMissileAttackDoesNotConsumeMeleeStreamOrPlayMeleeSound()
    {
        var sim = Room(3001); var monster = sim.Actors.Single(a => a.DoomEdNum == 3001);
        sim.Players.Single().X = Fixed.FromInt(500); monster.ReactionTime = 0;
        for (var i = 0; i < 17; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().TroopAttackRandomState);
        Assert.DoesNotContain(sim.DrainSoundRequests(), r => r.Sound == "imp/melee");
        Assert.Contains(sim.Actors, a => a is ProjectileActor);
    }

    [Fact]
    public void TroopStreamPreservesSequenceAndOtherStreamsAcrossSave()
    {
        var sim = Room(3001); var legacy = sim.CaptureState(); var combat = sim.CombatRandomState;
        var first = sim.NextTroopAttackDamage(); Assert.InRange(first, 3, 24); Assert.Equal(0, first % 3);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(131, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextTroopAttackDamage()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextTroopAttackDamage()).ToArray());
        Assert.Equal(combat, sim.CombatRandomState); Assert.Null(sim.CaptureState().SkelFistRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().TroopAttackRandomState);
        Assert.Equal(first, sim.NextTroopAttackDamage());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-troopattackrandom-size", error);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = type }] });
}
