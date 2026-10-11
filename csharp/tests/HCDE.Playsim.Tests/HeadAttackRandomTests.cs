using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class HeadAttackRandomTests
{
    [Theory]
    [InlineData(3005, false)]
    [InlineData(3005, true)]
    public void CacodemonConsumesNamedStreamOnlyForInRangeHits(int type, bool leavesRange)
    {
        var sim = Room(type); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        var player = sim.Players.Single(); monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "caco/attack" };
        ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        if (leavesRange) player.X = Fixed.FromInt(500);
        for (var i = 0; i < 10; i++) ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        Assert.Equal(!leavesRange, sim.CaptureState().HeadAttackRandomState.HasValue);
        Assert.Equal(!leavesRange, player.Health < 100);
        Assert.Equal(leavesRange ? 1 : 2, sim.DrainSoundRequests().Count(r => r.ActorId == monster.Id && r.Sound == "caco/attack"));
        Assert.Null(sim.CaptureState().SkelFistRandomState);
    }

    [Fact]
    public void DistantMissileAttackDoesNotConsumeMeleeStreamOrPlayMeleeSound()
    {
        var sim = Room(3005); var monster = sim.Actors.Single(a => a.DoomEdNum == 3005);
        sim.Players.Single().X = Fixed.FromInt(500); monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "caco/attack" };
        for (var i = 0; i < 17; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().HeadAttackRandomState);
        Assert.DoesNotContain(sim.DrainSoundRequests(), r => r.ActorId == monster.Id && r.Sound == "caco/attack");
        Assert.Contains(sim.Actors, a => a is ProjectileActor);
    }

    [Fact]
    public void HeadStreamPreservesSequenceAndOtherStreamsAcrossSave()
    {
        var sim = Room(3005); var legacy = sim.CaptureState(); var combat = sim.CombatRandomState;
        var first = sim.NextHeadAttackDamage(); Assert.InRange(first, 10, 60); Assert.Equal(0, first % 10);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(132, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextHeadAttackDamage()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextHeadAttackDamage()).ToArray());
        Assert.Equal(combat, sim.CombatRandomState); Assert.Null(sim.CaptureState().SkelFistRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().HeadAttackRandomState);
        Assert.Equal(first, sim.NextHeadAttackDamage());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-headattackrandom-size", error);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = type }] });
}
