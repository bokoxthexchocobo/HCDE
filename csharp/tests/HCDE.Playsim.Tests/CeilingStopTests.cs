using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class CeilingStopTests
{
    [Fact]
    public void CrushStopCannotRemovePausedMoverButCeilingStopCan()
    {
        var sim = Room(); Assert.True(Use(sim, 41, 7, 8, 10));
        Assert.True(Use(sim, 44, 7, 1));
        Assert.False(Use(sim, 44, 7, 2)); Assert.False(Use(sim, 44, 7, 1));
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
        Assert.False(Use(sim, 41, 7, 8, 10));
        Assert.True(Use(sim, 276, 7)); Assert.True(Use(sim, 41, 7, 8, 10));
    }

    [Fact]
    public void CrushStopMatchesCreationTagInsteadOfSectorTag()
    {
        var sim = Room(); Assert.True(Use(sim, 41, 0, 8, 10));
        Assert.False(Use(sim, 44, 7, 1)); sim.Tick(); Assert.Equal(9, sim.CeilingOf(0));
        Assert.False(Use(sim, 44, 0, 1));
        Assert.True(Use(sim, 44, 0x1000000, 1)); sim.Tick(); Assert.Equal(9, sim.CeilingOf(0));
        Assert.True(Use(sim, 276, 7)); Assert.True(Use(sim, 41, 7, 8, 10));
    }

    [Fact]
    public void CeilingStopRemovesDoorButPreservesIndependentFloor()
    {
        var sim = Room(); sim.Players.Single().Solid = false;
        Assert.True(Use(sim, 10, 7, 8)); Assert.True(Use(sim, 23, 7, 8, 4));
        Assert.False(Use(sim, 44, 7, 2)); Assert.True(Use(sim, 276, 7));
        sim.Tick(); Assert.Equal(8, sim.CeilingOf(0)); Assert.Equal(1, sim.FloorOf(0));
    }

    [Fact]
    public void ManualResumeRetainsOriginalMovementButReportsNoNewMover()
    {
        var sim = Room(); Assert.True(Use(sim, 41, 0, 8, 10));
        Assert.True(Use(sim, 44, 0x1000000, 1));
        Assert.False(Use(sim, 40, 0, 64, 2));
        sim.Tick(); Assert.Equal(9, sim.CeilingOf(0));
    }

    [Fact]
    public void TaggedCrusherDoesNotResumeManualMoverWithDifferentCreationTag()
    {
        var sim = Room(); Assert.True(Use(sim, 41, 0, 8, 10));
        Assert.True(Use(sim, 44, 0x1000000, 1));
        Assert.False(Use(sim, 42, 7, 8, 10)); sim.Tick(); Assert.Equal(8, sim.CeilingOf(0));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(0x1000000)]
    public void TaggedCrusherResumesByCreationTagEvenWithoutMatchingSector(int tag)
    {
        var sim = Room(); Assert.True(Use(sim, 41, tag == 7 ? 7 : 0, 8, 10));
        Assert.True(Use(sim, 44, tag, 1));
        Assert.True(Use(sim, 42, tag, 64, 10)); sim.Tick();
        Assert.Equal(9, sim.CeilingOf(0)); // Old raising direction and speed survive the crusher request.
    }

    [Fact]
    public void CeilingStopSucceedsForMissingTargetAndConsumesOneShotLine()
    {
        var sim = Room(); var line = new LevelLine { Special = 276, Arg0 = 999, PlayerUse = true };
        Assert.True(LineSpecials.ActivateMapLine(sim, sim.Players.Single(), line, true));
        Assert.Equal(0, line.Special); Assert.False(Use(sim, 44, 999, 2));
    }

    private static bool Use(AuthoritySimulation sim, int special, int tag, int arg1 = 0, int arg2 = 0) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine
        { Special = special, Arg0 = tag, Arg1 = arg1, Arg2 = arg2, SideBack = 0, PlayerUse = true }, true);
    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Format = MapDataFormat.HexenBinary, Sectors = [new LevelSector { Tag = 7, CeilingHeight = 8 }],
        Sides = [new LevelSide { Sector = 0 }], Things = [new LevelThing { Type = 1 }] });
}
