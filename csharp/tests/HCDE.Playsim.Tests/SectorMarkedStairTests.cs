using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SectorMarkedStairTests
{
    [Theory]
    [InlineData(26, 0)]
    [InlineData(26, 1)]
    [InlineData(26, 2)]
    [InlineData(27, 0)]
    [InlineData(27, 1)]
    [InlineData(27, 2)]
    [InlineData(31, 0)]
    [InlineData(31, 1)]
    [InlineData(31, 2)]
    [InlineData(32, 0)]
    [InlineData(32, 1)]
    [InlineData(32, 2)]
    public void MarkerChainIgnoresTextureAndLineOrientation(int special, int route)
    {
        var sim = Room(); var words = new List<int>();
        int[] args = [7, 8, 4, 0, 0];
        if (route == 2)
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
        else if (route == 1)
        {
            foreach (var arg in args) words.AddRange([3, arg]);
            words.AddRange([8, special]);
        }
        else { words.AddRange([13, special]); words.AddRange(args); }
        words.AddRange([62, 9, 10, 112, 7, 35, 1]);
        Add(sim, words.ToArray()); sim.Acs.Tick(sim);
        var sign = special is 26 or 31 ? -1 : 1;
        var sync = special is 31 or 32;
        sim.Tick(); Assert.Equal(sign, sim.FloorOf(0)); Assert.Equal(sign * (sync ? 2 : 1), sim.FloorOf(1));
        var ticks = sync ? 4 : 12;
        for (var i = 1; i < ticks - 1; i++) sim.Tick();
        Assert.Equal(128, sim.LightOf(0)); sim.Tick();
        Assert.Equal(sign * 4, sim.FloorOf(0)); Assert.Equal(sign * 8, sim.FloorOf(1));
        Assert.Equal(sign * 12, sim.FloorOf(2)); Assert.Equal(0, sim.FloorOf(3));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
    }

    [Theory]
    [InlineData(27)]
    [InlineData(32)]
    public void BusyMarkedStepStillAdvancesTraversalAndHeight(int special)
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 23, Arg0 = 8, Arg1 = 0, Arg2 = 1, PlayerUse = true }, true));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
        for (var i = 0; i < 12; i++) sim.Tick();
        Assert.Equal(4, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(1)); Assert.Equal(12, sim.FloorOf(2));
    }

    [Theory]
    [InlineData(26)]
    [InlineData(27)]
    [InlineData(31)]
    [InlineData(32)]
    public void MarkerChainResetRetainsOwnershipAndTagWaitUntilReturn(int special)
    {
        var sim = Room(); var sync = special is 31 or 32;
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 4, Arg3 = sync ? 5 : 0,
            Arg4 = sync ? 99 : 5, PlayerUse = true }, true));
        Add(sim, [62, 9, 10, 112, 7, 35, 1]); sim.Acs.Tick(sim);
        var sign = special is 26 or 31 ? -1 : 1;
        for (var i = 0; i < 4; i++) sim.Tick();
        Assert.Equal(sign * 4, sim.FloorOf(0));
        Assert.Equal(sign * (sync ? 12 : 4), sim.FloorOf(2));
        Assert.Equal(128, sim.LightOf(0));
        Assert.False(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        for (var i = 0; i < 3; i++) { sim.Tick(); Assert.Equal(128, sim.LightOf(0)); }
        sim.Tick();
        for (var i = 0; i < 3; i++) Assert.Equal(0, sim.FloorOf(i));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
    }

    private static void Add(AuthoritySimulation sim, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = 1, Code = bytes }); Assert.True(sim.Acs.TryExecute(1, []));
    }
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, Special = 26, CeilingHeight = 128, FloorPic = "A", LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 8, Special = 27, CeilingHeight = 128, FloorPic = "B" },
            new LevelSector { Index = 2, Tag = 9, Special = 26, CeilingHeight = 128, FloorPic = "C" },
            new LevelSector { Index = 3, Special = 26, CeilingHeight = 128, FloorPic = "A" }],
        Sides = Enumerable.Range(0, 4).Select(i => new LevelSide { Sector = i }).ToArray(),
        Lines = [new LevelLine { SideFront = 3, SideBack = 0, Flags = 4 },
            new LevelLine { SideFront = 1, SideBack = 0, Flags = 4 },
            new LevelLine { SideFront = 2, SideBack = 1, Flags = 4 }],
    });
}
