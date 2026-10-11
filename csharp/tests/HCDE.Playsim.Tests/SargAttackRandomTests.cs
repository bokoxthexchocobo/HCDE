using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SargAttackRandomTests
{
    [Theory]
    [InlineData(3002, false)]
    [InlineData(3002, true)]
    [InlineData(58, false)]
    [InlineData(58, true)]
    public void DemonAndSpectreConsumeNamedStreamOnlyForInRangeHits(int type, bool leavesRange)
    {
        var sim = Room(type); var monster = sim.Actors.Single(a => a.DoomEdNum == type);
        var player = sim.Players.Single(); monster.ReactionTime = 0;
        ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        if (leavesRange) player.X = Fixed.FromInt(500);
        for (var i = 0; i < 16; i++) ActorChaseActions.ExtChase(monster, true, false, playActive: false);
        Assert.Equal(!leavesRange, sim.CaptureState().SargAttackRandomState.HasValue);
        Assert.Equal(!leavesRange, player.Health < 100);
        Assert.Null(sim.CaptureState().SkelFistRandomState);
    }

    [Fact]
    public void SargStreamPreservesSequenceAndOtherStreamsAcrossSave()
    {
        var sim = Room(3002); var legacy = sim.CaptureState(); var combat = sim.CombatRandomState;
        var first = sim.NextSargAttackDamage(); Assert.InRange(first, 4, 40); Assert.Equal(0, first % 4);
        sim.RestoreState(sim.CaptureState()); var checksum = sim.Checksum;
        var bytes = SimSavegame.Write(sim); Assert.Equal(130, BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4)));
        var expected = Enumerable.Range(0, 16).Select(_ => sim.NextSargAttackDamage()).ToArray();
        SimSavegame.Apply(sim, bytes); Assert.Equal(checksum, sim.Checksum);
        Assert.Equal(expected, Enumerable.Range(0, 16).Select(_ => sim.NextSargAttackDamage()).ToArray());
        Assert.Equal(combat, sim.CombatRandomState); Assert.Null(sim.CaptureState().SkelFistRandomState);
        sim.RestoreState(legacy); Assert.Null(sim.CaptureState().SargAttackRandomState);
        Assert.Equal(first, sim.NextSargAttackDamage());
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length - 4), 15);
        Assert.False(SimSavegame.TryRead(bytes, out _, out var error)); Assert.Equal("save-sargattackrandom-size", error);
    }

    private static AuthoritySimulation Room(int type) => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1, X = 40 }, new LevelThing { Type = type }] });
}
