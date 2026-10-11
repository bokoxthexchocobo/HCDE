using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SPosAttackRandomTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ShotgunGuyConsumesStreamAndPlaysSoundOnlyWhenFiring(bool fire)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 9);
        monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/shotgun" };
        for (var i = 0; i < 11; i++) ActorChaseActions.ExtChase(monster, false, fire, playActive: false);
        Assert.Equal(fire, sim.CaptureState().SPosAttackRandomState.HasValue);
        var sounds = sim.DrainSoundRequests().Where(r => r.Sound == "custom/shotgun").ToArray();
        Assert.Equal(fire ? 1 : 0, sounds.Length);
        if (fire) Assert.Equal(1, sounds[0].Channel);
        if (fire)
        {
            var expected = Room();
            for (var pellet = 0; pellet < 3; pellet++) expected.NextSPosAttackPellet();
            Assert.Equal(expected.CaptureState().SPosAttackRandomState, sim.CaptureState().SPosAttackRandomState);
        }
        Assert.Null(sim.CaptureState().PosAttackRandomState);
        Assert.Null(sim.CaptureState().BruisAttackRandomState);
    }

    [Fact]
    public void ShotMatchesNativeDrawOrderAndRemainsIndependent()
    {
        var sim = Room(); var combat = sim.CombatRandomState;
        var random = NativeStateRandom.Seed(0, 0xe61ee544u);
        var first = (int)(NativeStateRandom.Next(ref random) & 255);
        var second = (int)(NativeStateRandom.Next(ref random) & 255);
        var damage = (1 + (int)NativeStateRandom.NextBounded(ref random, 5)) * 3;
        var actual = sim.NextSPosAttackPellet();
        Assert.Equal((first - second) * (22.5 / 256), actual.Spread);
        Assert.Equal(damage, actual.Damage);
        Assert.Equal(random, sim.CaptureState().SPosAttackRandomState);
        Assert.Equal(combat, sim.CombatRandomState);
    }

    [Fact]
    public void SavePreservesShotSequenceAndLegacyReset()
    {
        var sim = Room(); var legacy = sim.CaptureState(); var first = sim.NextSPosAttackPellet();
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(135, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextSPosAttackPellet()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextSPosAttackPellet()).ToArray());
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().SPosAttackRandomState);
        Assert.Equal(first, sim.NextSPosAttackPellet());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 135);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-sposattackrandom-header", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 9 }] }, rngSeed: 0);
}
