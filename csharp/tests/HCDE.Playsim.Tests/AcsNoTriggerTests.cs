using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsNoTriggerTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void SetGetAndCheckNormalizeBooleanValues(int value)
    {
        var sim = Room(); var actor = sim.Players.Single(); var expected = value != 0 ? 1 : 0;
        Run(sim, actor, [3, 0, 3, 23, 3, value, 245,
            3, 7, 3, 0, 3, 23, 246, 3, expected, 19, 5, 112, 1]);
        Assert.Equal(value != 0, actor.NoTrigger); Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 23, 3, value, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(1, sim.LightOf(0));
        Run(sim, actor, [3, 7, 3, 0, 3, 23, 3, expected == 0 ? -9 : 0, 351, 3, 22, 5, 112, 1]);
        Assert.Equal(0, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CrossingSuppressionPreservesOneShotLineUntilFlagCleared(bool reverse)
    {
        var sim = Room(); var actor = sim.Players.Single(); var line = sim.Level.Lines.Single();
        Run(sim, actor, [3, 0, 3, 23, 3, 1, 245, 1]);
        Cross(sim, actor, reverse);
        Assert.Equal(128, sim.LightOf(0)); Assert.Equal(112, line.Special);
        Run(sim, actor, [3, 0, 3, 23, 3, 0, 245, 1]);
        Cross(sim, actor, reverse);
        Assert.Equal(200, sim.LightOf(0)); Assert.Equal(0, line.Special);
    }

    [Fact]
    public void UseActivationRemainsAvailableWithNoTriggerFlag()
    {
        var sim = Room(); var actor = sim.Players.Single(); var line = sim.Level.Lines.Single();
        line.PlayerUse = true; actor.X = Fixed.FromInt(64); actor.Angle = BamAngle.FromDegrees(180);
        Run(sim, actor, [3, 0, 3, 23, 3, 1, 245, 1]);
        sim.QueueCommand(0, new PlayerCommand { Use = true }); sim.Tick();
        Assert.Equal(200, sim.LightOf(0)); Assert.Equal(0, line.Special);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingOrDestroyedTargetIgnoresWritesAndReturnsZero(bool destroyed)
    {
        var sim = Room(); var actor = sim.Players.Single(); if (destroyed) actor.Destroy();
        var tid = destroyed ? 0 : 999;
        Run(sim, actor, [3, tid, 3, 23, 3, 1, 245, 3, 7, 3, tid, 3, 23, 246, 5, 112, 1]);
        Assert.False(actor.NoTrigger); Assert.Equal(0, sim.LightOf(0));
    }

    [Fact]
    public void FlagParticipatesInRestingChecksum()
    {
        var first = Room(); var second = Room(); second.Players.Single().NoTrigger = true;
        first.Tick(); second.Tick(); Assert.NotEqual(first.Checksum, second.Checksum);
    }

    private static void Cross(AuthoritySimulation sim, Actor actor, bool reverse)
    {
        actor.X = Fixed.FromInt(reverse ? 64 : 0); actor.RememberPosition();
        actor.X = Fixed.FromInt(reverse ? 0 : 64); LineSpecials.ActivateCrossings(sim);
    }
    private static void Run(AuthoritySimulation sim, Actor actor, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); sim.Acs.TryExecute(1, [], actor); sim.Acs.Tick(sim);
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 256, LightLevel = 128 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 0 }],
        Lines = [new LevelLine { X1 = 32, X2 = 32, Y1 = -128, Y2 = 128, SideFront = 0, SideBack = 1,
            Special = 112, Arg0 = 7, Arg1 = 200, PlayerCross = true }],
        Things = [new LevelThing { Type = 1 }],
    });
}
