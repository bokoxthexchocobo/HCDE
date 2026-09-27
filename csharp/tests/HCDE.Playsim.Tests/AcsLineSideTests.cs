using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLineSideTests
{
    [Theory]
    [InlineData(-8, 1)]
    [InlineData(8, 0)]
    public void UseCapturesActivatorSide(int x, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.X = Fixed.FromInt(x);
        Add(sim, 1, [3, 7, 80, 5, 112, 1]);
        Assert.True(LineSpecials.ActivateMapLine(sim, actor, sim.Level.Lines[0], true));
        sim.Acs.Tick(sim); Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-8, 1)]
    [InlineData(8, 0)]
    public void CrossingCapturesSideBeforeMovement(int x, int expected)
    {
        var sim = Room(); var actor = sim.Players.Single(); actor.X = Fixed.FromInt(x); actor.RememberPosition();
        actor.X = Fixed.FromInt(-x);
        Add(sim, 1, [3, 7, 80, 5, 112, 1]);
        LineSpecials.ActivateCrossings(sim); sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NestedExecutionAndResumeRetainSide(bool back)
    {
        var sim = Room(); Add(sim, 1, [2, 13, 226, 2, 0, 0, 0, 0, 1]);
        Add(sim, 2, [3, 7, 80, 5, 112, 1]);
        sim.Acs.TryExecute(1, [], backSide: back); sim.Acs.Tick(sim);
        Assert.True(sim.Acs.TryExecute(1, [], backSide: !back));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim);
        Assert.Equal(back ? 1 : 0, sim.LightOf(0));
    }

    [Fact]
    public void SideParticipatesInChecksum()
    {
        var first = Room(); var second = Room();
        Add(first, 1, [1]); Add(second, 1, [1]);
        first.Acs.TryExecute(1, []); second.Acs.TryExecute(1, [], backSide: true);
        Assert.NotEqual(first.Acs.Checksum, second.Acs.Checksum);
    }

    private static void Add(AuthoritySimulation sim, int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, LightLevel = 128 }],
        Lines = [new LevelLine { X1 = 0, Y1 = -64, X2 = 0, Y2 = 64, Special = 80, Arg0 = 1,
            PlayerUse = true, PlayerCross = true }],
        Things = [new LevelThing { Type = 1 }],
    });
}
