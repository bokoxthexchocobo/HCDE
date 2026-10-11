using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BruisAttackRandomTests
{
    [Theory]
    [InlineData(3003, false)]
    [InlineData(3003, true)]
    [InlineData(69, false)]
    [InlineData(69, true)]
    public void BaronAndKnightConsumeNamedStreamOnlyForInRangeHits(int type, bool leavesRange)
    {
        var sim = Room(type); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        var player = sim.Players.Single(); monster.ReactionTime = 0;
        ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        if (leavesRange) player.X = Fixed.FromInt(500);
        for (var i = 0; i < 16; i++) ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        Assert.Equal(!leavesRange, sim.CaptureState().BruisAttackRandomState.HasValue);
        Assert.Equal(!leavesRange, player.Health < 100);
        Assert.Equal(leavesRange ? 0 : 1, sim.DrainSoundRequests().Count(r => r.Sound == "baron/melee"));
        Assert.Null(sim.CaptureState().SkelFistRandomState);
    }

    [Theory]
    [InlineData(3003)]
    [InlineData(69)]
    public void DistantMissileAttackDoesNotConsumeMeleeStreamOrPlayMeleeSound(int type)
    {
        var sim = Room(type); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        sim.Players.Single().X = Fixed.FromInt(500); monster.ReactionTime = 0;
        for (var i = 0; i < 17; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().BruisAttackRandomState);
        Assert.DoesNotContain(sim.DrainSoundRequests(), r => r.Sound == "baron/melee");
        Assert.Contains(sim.Actors, a => a is ProjectileActor);
    }

    [Fact]
    public void BaronAndKnightAdvanceOneSharedStream()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = 3003 }, new LevelThing { Type = 69 }] });
        var expected = Room(3003);
        expected.NextBruisAttackDamage(); expected.NextBruisAttackDamage();
        foreach (var monster in sim.Actors.Where(a => a.DoomEdNum is 3003 or 69).ToArray())
        {
            monster.ReactionTime = 0;
            for (var i = 0; i < 17; i++) ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        }
        Assert.Equal(expected.CaptureState().BruisAttackRandomState, sim.CaptureState().BruisAttackRandomState);
    }

    [Fact]
    public void BruisStreamPreservesSequenceAndOtherStreamsAcrossSave()
    {
        var sim = Room(3003); var legacy = sim.CaptureState(); var combat = sim.CombatRandomState;
        var first = sim.NextBruisAttackDamage(); Assert.InRange(first, 10, 80); Assert.Equal(0, first % 10);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(133, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextBruisAttackDamage()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextBruisAttackDamage()).ToArray());
        Assert.Equal(combat, sim.CombatRandomState); Assert.Null(sim.CaptureState().SkelFistRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().BruisAttackRandomState);
        Assert.Equal(first, sim.NextBruisAttackDamage());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-bruisattackrandom-size", error);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = type }] });
}
