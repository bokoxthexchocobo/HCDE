using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsLineActivationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ExtendedLineForwardsArgumentsInOrderAndHonorsActivationAndRepeat(bool use, bool repeat)
    {
        var sim = Room(); Register(sim, 3, 3, 7, 28, 0, 3, 100, 16, 28, 1, 3, 10, 16, 14, 28, 2, 14, 5, 112, 1);
        var line = Line(use, repeat, a: 2, b: 3, c: -4);
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, !use));
        Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, use));
        Assert.Equal(repeat ? 80 : 0, line.Special);
        sim.Acs.Tick(sim); Assert.Equal(226, sim.LightOf(0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void NonzeroMapRequestDoesNotRunLocallyOrConsumeLine(int map)
    {
        var sim = Room(); Register(sim, 0, 10, 112, 7, 35, 1);
        var line = Line(map: map);
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(80, line.Special); Assert.Equal(0, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.Equal(128, sim.LightOf(0));
    }

    [Fact]
    public void MissingScriptDoesNotConsumeLine()
    {
        var sim = Room(); var line = Line();
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true)); Assert.Equal(80, line.Special);
    }

    [Fact]
    public void ActiveExecuteIsRejectedWithoutReplacingArgumentsAndCanRestartAfterCompletion()
    {
        var sim = Room(); Register(sim, 1, 56, 1, 3, 7, 28, 0, 5, 112, 1);
        var line = Line(repeat: true, a: 19);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        line = Line(repeat: true, a: 35); var checksum = sim.Acs.Checksum;
        Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(checksum, sim.Acs.Checksum); Assert.Equal(1, sim.Acs.RunningCount);
        sim.Acs.Tick(sim); Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        sim.Acs.Tick(sim); Assert.Equal(19, sim.LightOf(0));
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        sim.Acs.Tick(sim); sim.Acs.Tick(sim); Assert.Equal(35, sim.LightOf(0));
    }

    [Fact]
    public void RejectedDuplicateDoesNotConsumeAnotherOneShotLine()
    {
        var sim = Room(); Register(sim, 0, 56, 5, 1);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), Line(repeat: true), true));
        var other = Line(); Assert.False(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), other, true));
        Assert.Equal(80, other.Special);
    }

    [Fact]
    public void LineArgumentsRespectDeclaredCountAndZeroFillMissingFourthArgument()
    {
        var sim = Room(); Register(sim, 4, 3, 7, 28, 0, 28, 1, 14, 28, 2, 14, 28, 3, 14, 5, 112, 1);
        var line = Line(a: 10, b: 20, c: 30);
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        sim.Acs.Tick(sim); Assert.Equal(60, sim.LightOf(0));
    }

    private static LevelLine Line(bool use = true, bool repeat = false, int map = 0, int a = 0, int b = 0, int c = 0) => new()
    { Special = 80, Arg0 = 1, Arg1 = map, Arg2 = a, Arg3 = b, Arg4 = c, PlayerUse = use, PlayerCross = !use, Repeat = repeat };
    private static void Register(AuthoritySimulation sim, int argumentCount, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes, ArgumentCount = argumentCount });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Index = 0, Tag = 7, LightLevel = 128, CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }] });
}
