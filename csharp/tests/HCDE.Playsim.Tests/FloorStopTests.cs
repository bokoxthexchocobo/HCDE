using System.Buffers.Binary;
using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class FloorStopTests
{
    [Theory]
    [InlineData(0, 28)]
    [InlineData(1, 28)]
    [InlineData(2, 28)]
    [InlineData(0, 99)]
    [InlineData(1, 99)]
    [InlineData(2, 99)]
    [InlineData(0, 279)]
    [InlineData(1, 279)]
    [InlineData(2, 279)]
    public void CrushStopOnlyRemovesNativeRaiseAndCrush(int route, int special)
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 10, Arg3 = 1, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(1, sim.FloorOf(0));
        Add(sim, 1, [62, 7, 10, 112, 7, 35, 1]);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        if (route == 2)
        {
            var line = new LevelLine { Special = 46, Arg0 = 7, PlayerUse = true };
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
            Assert.Equal(0, line.Special);
        }
        else
        {
            Add(sim, 2, route == 0 ? [9, 46, 7, 1] : [3, 7, 4, 46, 1]);
            Assert.True(sim.Acs.TryExecute(2, [])); sim.Acs.Tick(sim);
        }
        sim.Tick();
        Assert.Equal(special == 28 ? 1 : 2, sim.FloorOf(0));
        Assert.Equal(special == 28 ? 35 : 128, sim.LightOf(0));
        Assert.Equal(special == 28, LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
    }

    [Theory]
    [InlineData(46, 7, 1)]
    [InlineData(46, 999, 1)]
    [InlineData(46, 0, 0)]
    [InlineData(275, 7, 1)]
    [InlineData(275, 999, 1)]
    [InlineData(275, 0, 0)]
    public void StopResultSucceedsWithoutMoverButRequiresManualLineContext(int special, int tag, int expected)
    {
        var sim = Room();
        Add(sim, 1, [3, 7, 3, tag, 3, 0, 3, 0, 3, 0, 3, 0, 263, special, 5, 112, 1]);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.Equal(expected, sim.LightOf(0));
    }

    [Theory]
    [InlineData(46)]
    [InlineData(275)]
    public void ManualStopUsesBackSectorAndPreservesOtherTaggedMover(int special)
    {
        var sim = Room();
        foreach (var tag in new[] { 7, 8 })
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = 28, Arg0 = tag, Arg1 = 8, Arg2 = 10, PlayerUse = true }, true));
        sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = special, SideFront = 0, SideBack = 1, PlayerUse = true }, true));
        sim.Tick(); Assert.Equal(2, sim.FloorOf(0)); Assert.Equal(1, sim.FloorOf(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void FloorStopRemovesDifferentFloorMoversButPreservesCeiling(int route)
    {
        foreach (var special in new[] { 23, 28, 99, 279, 62 })
        {
            var sim = Room(special == 62 ? 16 : 0);
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = special, Arg0 = 7, Arg1 = 8, Arg2 = 10, Arg3 = 0, PlayerUse = true }, true));
            Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
            { Special = 40, Arg0 = 7, Arg1 = 8, Arg2 = 8, PlayerUse = true }, true));
            sim.Tick(); var floor = sim.FloorOf(0); var ceiling = sim.CeilingOf(0);
            if (route == 2)
            {
                var line = new LevelLine { Special = 275, Arg0 = 7, PlayerUse = true };
                Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), line, true));
                Assert.Equal(0, line.Special);
            }
            else
            {
                Add(sim, 1, route == 0 ? [9, 275, 7, 1] : [3, 7, 4, 275, 1]);
                Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
            }
            sim.Tick(); Assert.Equal(floor, sim.FloorOf(0)); Assert.Equal(ceiling - 1, sim.CeilingOf(0));
            Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 7));
        }
    }

    [Fact]
    public void StoppedStairReleasesPlaneAndTagWaitButRetainsChainLock()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 217, Arg0 = 7, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
        sim.Tick();
        Add(sim, 1, [62, 8, 10, 112, 7, 35, 1]);
        Assert.True(sim.Acs.TryExecute(1, [])); sim.Acs.Tick(sim);
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 275, Arg0 = 8, PlayerUse = true }, true));
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(4, sim.FloorOf(0)); Assert.Equal(1, sim.FloorOf(1));
        Assert.Equal(35, sim.LightOf(0)); Assert.Equal(0, sim.Acs.RunningCount);
        Assert.False(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 217, Arg0 = 8, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
        Assert.True(LineSpecials.Execute(sim, null, LineSpecials.FloorRaise, 8));
    }

    [Fact]
    public void StoppingSeedLetsRemainingStairFinishAndUnlock()
    {
        var sim = Room();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 217, Arg0 = 7, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
        sim.Tick();
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 275, Arg0 = 7, PlayerUse = true }, true));
        for (var i = 0; i < 10; i++) sim.Tick();
        Assert.Equal(1, sim.FloorOf(0)); Assert.Equal(8, sim.FloorOf(1));
        Assert.True(LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 217, Arg0 = 8, Arg1 = 8, Arg2 = 4, PlayerUse = true }, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RestartedSeedGetsIndependentChainCleanup(bool stopOldStep)
    {
        var sim = Room();
        bool Start(int tag, int step) => LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 217, Arg0 = tag, Arg1 = 8, Arg2 = step, PlayerUse = true }, true);
        bool Stop(int tag) => LineSpecials.ActivateMapLine(sim, new PlayerPawn(), new LevelLine
        { Special = 275, Arg0 = tag, PlayerUse = true }, true);
        Assert.True(Start(7, 8)); sim.Tick();
        Assert.True(Stop(7));
        if (stopOldStep) Assert.True(Stop(8));
        // The old second step is either still moving or retains its native lock.
        // The restarted seed must complete independently in both cases.
        Assert.True(Start(7, 2)); sim.Tick(); sim.Tick();
        Assert.Equal(3, sim.FloorOf(0));
        Assert.Equal(stopOldStep ? 1 : 3, sim.FloorOf(1));
        Assert.True(Start(7, 2)); sim.Tick(); sim.Tick();
        Assert.Equal(5, sim.FloorOf(0));
        Assert.False(Start(8, 2));
        Assert.True(Stop(7));
        if (!stopOldStep)
        {
            for (var i = 0; i < 12; i++) sim.Tick();
            Assert.Equal(16, sim.FloorOf(1));
            Assert.True(Start(8, 2));
        }
    }

    private static AuthoritySimulation Room(int floor = 0) => AuthoritySimulation.Start(new PlayLevel
    {
        Format = MapDataFormat.HexenBinary,
        Sectors = [new LevelSector { Tag = 7, FloorHeight = floor, CeilingHeight = 32, LightLevel = 128 },
            new LevelSector { Index = 1, Tag = 8, CeilingHeight = 32 }],
        Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }],
        Lines = [new LevelLine { SideFront = 0, SideBack = 1, Flags = 4 }],
    });

    private static void Add(AuthoritySimulation sim, int number, int[] words)
    {
        var bytes = new byte[words.Length * 4];
        for (var i = 0; i < words.Length; i++) BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i * 4), words[i]);
        sim.Acs.Add(new AcsProgram { Number = number, Code = bytes });
    }
}
