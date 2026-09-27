using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StairObstructionTests
{
    [Theory]
    [InlineData(3, 4, false)]
    [InlineData(4, 8, false)]
    [InlineData(3, 0, true)]
    [InlineData(4, 0, true)]
    public void LaterStepUsesVariantSpecificCrushing(int step, int expected, bool marked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Special = 27, CeilingHeight = 128, FloorPic = "STEP" },
                new LevelSector { Index = 1, Tag = 7, Special = 26, CeilingHeight = 128, FloorPic = "STEP" }],
            Sides = [new LevelSide { Sector = 1 }, new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { Flags = LevelLine.TwoSidedFlag, SideFront = 0, SideBack = 1 },
                new LevelLine { X1 = -128, Y1 = -128, X2 = 128, Y2 = -128, SideFront = 1, SideBack = -1 },
                new LevelLine { X1 = 128, Y1 = -128, X2 = 128, Y2 = 128, SideFront = 1, SideBack = -1 },
                new LevelLine { X1 = 128, Y1 = 128, X2 = -128, Y2 = 128, SideFront = 1, SideBack = -1 },
                new LevelLine { X1 = -128, Y1 = 128, X2 = -128, Y2 = -128, SideFront = 1, SideBack = -1 }],
            Things = [new LevelThing { Type = 1 }]
        });
        for (var i = 0; i < 4; i++) sim.Tick();
        var player = sim.Players.Single(); Assert.Equal(0, player.SectorIndex); player.Height = Fixed.FromInt(125);
        var special = marked ? 27 : 217;
        Assert.True(Start(sim, special, 32, step));
        LineSpecials.TickMotions(sim); Assert.Equal(marked ? 0 : 4, sim.FloorOf(0));
        LineSpecials.TickMotions(sim); Assert.Equal(expected, sim.FloorOf(0));
        if (marked)
        {
            Assert.Equal(100, player.Health);
            Assert.False(Start(sim, special, 32, step));
            player.Solid = false;
            LineSpecials.TickMotions(sim); LineSpecials.TickMotions(sim);
            Assert.Equal(step * 2, sim.FloorOf(0));
        }
        Assert.True(Start(sim, special, 32, step));
    }

    [Theory]
    [InlineData(217, 8, 1)]
    [InlineData(273, 8, 1)]
    [InlineData(217, 24, 0)]
    [InlineData(273, 24, 0)]
    public void BlockedDestinationReleasesMoverAfterRollingBack(int special, int speed, int expectedFloor)
    {
        var sim = Room(); Assert.True(Start(sim, special, speed, 2));
        sim.Tick(); if (speed == 8) sim.Tick();
        Assert.Equal(expectedFloor, sim.FloorOf(0));
        Assert.True(Start(sim, special, speed, 2));
    }

    [Theory]
    [InlineData(217)]
    [InlineData(273)]
    public void IntermediateObstructionRetainsMoverUntilSpaceClears(int special)
    {
        var sim = Room(); Assert.True(Start(sim, special, 8, 4));
        sim.Tick(); sim.Tick(); Assert.Equal(1, sim.FloorOf(0));
        Assert.False(Start(sim, special, 8, 4));
        sim.Players.Single().Solid = false;
        for (var i = 0; i < 3; i++) sim.Tick();
        Assert.Equal(4, sim.FloorOf(0)); Assert.True(Start(sim, special, 8, 4));
    }

    [Fact]
    public void BlockedBuildDestinationStillWaitsForResetAndReturns()
    {
        var sim = Room(); Assert.True(Start(sim, 217, 8, 2, reset: 5));
        sim.Tick(); sim.Tick(); Assert.Equal(1, sim.FloorOf(0));
        Assert.False(Start(sim, 217, 8, 2));
        sim.Tick(); sim.Tick(); Assert.Equal(1, sim.FloorOf(0));
        sim.Tick(); Assert.Equal(0, sim.FloorOf(0)); Assert.True(Start(sim, 217, 8, 2));
    }

    private static bool Start(AuthoritySimulation sim, int special, int speed, int step, int reset = 0) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
        { Special = special, Arg0 = 7, Arg1 = speed, Arg2 = step, Arg4 = reset, PlayerUse = true }, true);
    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Format = MapDataFormat.HexenBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128 }],
            Things = [new LevelThing { Type = 1 }]
        });
        sim.Players.Single().Height = Fixed.FromInt(127);
        return sim;
    }
}
