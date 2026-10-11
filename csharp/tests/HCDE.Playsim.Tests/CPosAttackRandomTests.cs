using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CPosAttackRandomTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChaingunnerConsumesStreamAndPlaysSoundOnlyWhenFiring(bool fire)
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 65);
        monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/chaingun" };
        for (var i = 0; i < 11; i++) ActorChaseActions.ExtChase(monster, false, fire, playActive: false);
        Assert.Equal(fire, sim.CaptureState().CPosAttackRandomState.HasValue);
        var sounds = sim.DrainSoundRequests().Where(r => r.Sound == "custom/chaingun").ToArray();
        Assert.Equal(fire ? 1 : 0, sounds.Length);
        if (fire) Assert.Equal(1, sounds[0].Channel);
        Assert.Null(sim.CaptureState().BruisAttackRandomState);
    }

    [Fact]
    public void SecondShotAdvancesStreamAndPlaysAnotherConfiguredSound()
    {
        var sim = Room(); var monster = sim.Actors.Single(a => a.DoomEdNum == 65);
        monster.ReactionTime = 0;
        monster.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Attack] = "custom/chaingun" };
        for (var i = 0; i < 15; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = Room(); expected.NextCPosAttackShot(); expected.NextCPosAttackShot();
        Assert.Equal(expected.CaptureState().CPosAttackRandomState, sim.CaptureState().CPosAttackRandomState);
        Assert.Equal(2, sim.DrainSoundRequests().Count(r => r.Sound == "custom/chaingun"));
        Assert.Null(sim.CaptureState().PosAttackRandomState);
        Assert.Null(sim.CaptureState().SPosAttackRandomState);
    }

    [Fact]
    public void ShotMatchesNativeDrawOrderAndRemainsIndependent()
    {
        var sim = Room(); var combat = sim.CombatRandomState;
        var random = NativeStateRandom.Seed(0, 0xc1b064acu);
        var first = (int)(NativeStateRandom.Next(ref random) & 255);
        var second = (int)(NativeStateRandom.Next(ref random) & 255);
        var damage = (1 + (int)NativeStateRandom.NextBounded(ref random, 5)) * 3;
        var actual = sim.NextCPosAttackShot();
        Assert.Equal((first - second) * (22.5 / 256), actual.Spread);
        Assert.Equal(damage, actual.Damage);
        Assert.Equal(random, sim.CaptureState().CPosAttackRandomState);
        Assert.Equal(combat, sim.CombatRandomState);
    }

    [Fact]
    public void SavePreservesShotSequenceAndLegacyReset()
    {
        var sim = Room(); var legacy = sim.CaptureState(); var first = sim.NextCPosAttackShot();
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(136, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextCPosAttackShot()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextCPosAttackShot()).ToArray());
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().CPosAttackRandomState);
        Assert.Equal(first, sim.NextCPosAttackShot());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 16), 136);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-cposattackrandom-header", error);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 65 }] }, rngSeed: 0);
}
