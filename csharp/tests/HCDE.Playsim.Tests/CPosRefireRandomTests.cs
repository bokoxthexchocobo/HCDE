using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CPosRefireRandomTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RefireMatchesNativeThresholdAndConsumesEvenWhenVisible(bool visible)
    {
        var sim = Room(); var combat = sim.CombatRandomState;
        var random = NativeStateRandom.Seed(0, 0x59106a10u);
        var continued = 0; var stopped = 0;
        for (var i = 0; i < 256; i++)
        {
            var expected = (NativeStateRandom.Next(ref random) & 255) < 40 || visible;
            Assert.Equal(expected, sim.CheckCPosRefire(visible));
            if (expected) continued++; else stopped++;
        }
        Assert.True(continued > 0);
        Assert.Equal(visible, stopped == 0);
        Assert.Equal(random, sim.CaptureState().CPosRefireRandomState);
        Assert.Equal(combat, sim.CombatRandomState);
        Assert.Null(sim.CaptureState().CPosAttackRandomState);
    }

    [Theory]
    [InlineData(65, 18, 40)]
    [InlineData(68, 28, 10)]
    public void AttackLoopDrawsAtNativeRefireStateEntry(int type, int refireTic, byte threshold)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type }],
        }, rngSeed: 0);
        var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        monster.ReactionTime = 0; sim.Players.Single().Invulnerable = true;
        for (var i = 0; i < refireTic; i++) ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Null(sim.CaptureState().CPosRefireRandomState);
        ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        var expected = Room(); expected.CheckCPosRefire(true, threshold);
        Assert.Equal(expected.CaptureState().CPosRefireRandomState, sim.CaptureState().CPosRefireRandomState);
        Assert.Equal(MonsterMode.Recovery, monster.Brain!.Mode);
    }

    [Fact]
    public void SavePreservesRefireDecisionsAndLegacyReset()
    {
        var sim = Room(); var legacy = sim.CaptureState(); var first = sim.CheckCPosRefire(false);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim);
        Assert.Equal(137, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 32).Select(_ => sim.CheckCPosRefire(false)).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 32).Select(_ => sim.CheckCPosRefire(false)).ToArray());
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().CPosRefireRandomState);
        Assert.Equal(first, sim.CheckCPosRefire(false));
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error));
        Assert.Equal("save-cposrefirerandom-size", error);
    }

    [Theory]
    [InlineData(65, 18, 10)]
    [InlineData(7, 28, 20)]
    [InlineData(68, 28, 20)]
    [InlineData(84, 34, 10)]
    public void SaveAfterSuccessfulRefirePreservesOneTicDelayWithoutAnotherDraw(int type, int refireTic, int repeatFrom)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = type }],
        });
        var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        monster.ReactionTime = 0;
        sim.Players.Single().Invulnerable = true;
        for (var i = 0; i <= refireTic; i++)
            ActorChaseActions.ExtChase(monster, false, true, playActive: false);
        Assert.Equal(refireTic, (type == 68 ? monster.Brain!.CaptureProfileAttack() : monster.Brain!.CaptureRepeatAttack())!.Value.AttackTic);
        var random = sim.CaptureState().CPosRefireRandomState;
        Assert.NotNull(random);
        var bytes = SimSavegame.Write(sim);
        for (var pass = 0; pass < 2; pass++)
        {
            ActorChaseActions.ExtChase(monster, false, true, playActive: false);
            Assert.Equal(repeatFrom, (type == 68 ? monster.Brain!.CaptureProfileAttack() : monster.Brain!.CaptureRepeatAttack())!.Value.AttackTic);
            Assert.Equal(random, sim.CaptureState().CPosRefireRandomState);
            SimSavegame.Apply(sim, bytes);
            monster = sim.Actors.Single(a => a.DoomEdNum == type);
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 500 }, new LevelThing { Type = 65 }] }, rngSeed: 0);
}
