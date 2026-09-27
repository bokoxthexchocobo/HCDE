using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class AcsDoorTests
{
    [Theory]
    [InlineData(false, 11)]
    [InlineData(true, 11)]
    [InlineData(false, 12)]
    [InlineData(true, 12)]
    public void OpenAndRaiseForwardSpeedAndDelayAndReleaseTagWait(bool stack, int special)
    {
        var sim = OpeningRoom(); var words = new List<int>(); int[] args = [7, 16, special == 12 ? 2 : 0, 0];
        if (stack) { foreach (var arg in args) words.AddRange([3, arg]); words.AddRange([7, special]); }
        else { words.AddRange([12, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        sim.Tick(); Assert.Equal(6, sim.CeilingOf(0)); Assert.Equal(128, sim.LightOf(0));
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        if (special == 12)
        {
            Assert.Equal(128, sim.LightOf(0)); sim.Tick(); sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
            for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
            sim.Tick(); Assert.Equal(0, sim.CeilingOf(0));
        }
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(11, 7, 0)]
    [InlineData(12, 2, 7)]
    [InlineData(12, -1, 0)]
    public void MapAndScriptBothRejectUnsupportedOpenRaiseArguments(int special, int arg2, int arg3)
    {
        var sim = OpeningRoom(); var playerSim = AuthoritySimulation.Start(new PlayLevel
        { Format = MapDataFormat.HexenBinary, Things = [new LevelThing { Type = 1 }], Sectors = sim.Level.Sectors,
            Sides = sim.Level.Sides, Lines = sim.Level.Lines });
        var line = new LevelLine { Special = special, Arg0 = 7, Arg1 = 16, Arg2 = arg2, Arg3 = arg3, PlayerUse = true };
        var checksum = playerSim.Checksum;
        Assert.False(LineSpecials.ActivateMapLine(playerSim, playerSim.Players.Single(), line, true));
        Assert.Equal(special, line.Special); Assert.Equal(checksum, playerSim.Checksum);
        Add(sim, 3, 7, 3, 7, 3, 16, 3, arg2, 3, arg3, 3, 0, 263, special, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(0, sim.LightOf(0)); Assert.Equal(4, sim.CeilingOf(0));
    }

    private static AuthoritySimulation OpeningRoom() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, CeilingHeight = 4, LightLevel = 128 }, new LevelSector { Index = 1, CeilingHeight = 12 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1 }]
    });

    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 10)]
    [InlineData(false, 249)]
    [InlineData(true, 249)]
    public void DirectAndStackDoorCallsBlockTagWaitUntilMotionFinishes(bool stack, int special)
    {
        var sim = Room(); var args = new[] { 7, 16, special == 249 ? 1 : 0, 0 }; var words = new List<int>();
        if (stack)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([7, special]);
        }
        else { words.AddRange([12, special]); words.AddRange(args); }
        words.AddRange([62, 7, 10, 112, 7, 35, 1]); Add(sim, words.ToArray());
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        var ticks = special == 249 ? 12 : 4;
        for (var i = 0; i < ticks - 1; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick(); Assert.Equal(35, sim.LightOf(0)); Assert.Equal(special == 249 ? 8 : 0, sim.CeilingOf(0));
        Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(263, 7, 16, 0, 1)]
    [InlineData(382, 7, 16, 0, 1)]
    [InlineData(263, 999, 16, 0, 0)]
    [InlineData(382, 0, 16, 0, 0)]
    [InlineData(263, 7, 0, 0, 0)]
    [InlineData(382, 7, 16, 7, 0)]
    public void ResultFormsReportDoorSuccessAndPreserveLowerStack(int opcode, int tag, int speed, int lightTag, int expected)
    {
        var sim = Room(); Add(sim, 3, 7, 3, tag, 3, speed, 3, lightTag, 3, 0, 3, 0, opcode, 10, 5, 112, 1);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.LightOf(0)); sim.Tick(); Assert.Equal(expected == 1 ? 6 : 8, sim.CeilingOf(0));
    }

    [Fact]
    public void TruncatedDirectDoorCallDoesNotStartMovement()
    {
        var sim = Room(); Add(sim, 10, 10, 7);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim); sim.Tick();
        Assert.Equal(8, sim.CeilingOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    private static void Add(AuthoritySimulation sim, params int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes });
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 8, LightLevel = 128 }] });
}
