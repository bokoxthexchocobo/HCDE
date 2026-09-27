using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class StairActionsTests
{
    private static AuthoritySimulation Room(bool differentTexture = false, bool reversed = false, bool cycle = false) =>
        AuthoritySimulation.Start(new PlayLevel {
            Format = MapDataFormat.DoomBinary,
            Sectors = [new LevelSector { Tag = 7, CeilingHeight = 128, FloorPic = "STEP" },
                new LevelSector { Index = 1, CeilingHeight = 128, FloorPic = differentTexture ? "OTHER" : "step" },
                new LevelSector { Index = 2, CeilingHeight = 128, FloorPic = "STEP" }],
            Sides = [new LevelSide { Sector = 0 }, new LevelSide { Sector = 1 }, new LevelSide { Sector = 2 }],
            Lines = [new LevelLine { SideFront = reversed ? 1 : 0, SideBack = reversed ? 0 : 1 },
                new LevelLine { SideFront = 1, SideBack = 2 },
                new LevelLine { SideFront = 2, SideBack = cycle ? 0 : -1 }],
            Things = [new LevelThing { Type = 1 }],
        });

    private static bool Start(AuthoritySimulation sim, int special = 256, bool use = false) =>
        LineSpecials.ActivateMapLine(sim, sim.Players.Single(), new LevelLine { Special = special, Tag = 7 }, use);

    [Theory]
    [InlineData(8, false, 8, 0.25)]
    [InlineData(7, true, 8, 0.25)]
    [InlineData(100, false, 16, 4)]
    [InlineData(127, true, 16, 4)]
    public void DoomStairsBuildAscendingChain(int special, bool use, double height, double speed)
    {
        var sim = Room();
        Assert.True(Start(sim, special, use));
        sim.Tick();
        Assert.Equal(speed, sim.FloorOf(0)); Assert.Equal(speed, sim.FloorOf(1)); Assert.Equal(speed, sim.FloorOf(2));
        for (var i = 0; i < height * 3 / speed; i++) sim.Tick();
        Assert.Equal(height, sim.FloorOf(0)); Assert.Equal(height * 2, sim.FloorOf(1)); Assert.Equal(height * 3, sim.FloorOf(2));
    }

    [Fact]
    public void WholeChainStaysLockedUntilLastStepCompletes()
    {
        var sim = Room(); Assert.True(Start(sim));
        for (var i = 0; i < 32; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.False(Start(sim));
        for (var i = 0; i < 64; i++) sim.Tick();
        Assert.Equal(24, sim.FloorOf(2)); Assert.True(Start(sim));
        sim.Tick(); Assert.Equal(8.25, sim.FloorOf(0));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void TextureAndSidednessStopChainTraversal(bool different, bool reversed)
    {
        var sim = Room(different, reversed); Assert.True(Start(sim));
        for (var i = 0; i < 96; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(0, sim.FloorOf(1)); Assert.Equal(0, sim.FloorOf(2));
    }

    [Fact]
    public void CyclicSectorGraphBuildsEachStepOnce()
    {
        var sim = Room(cycle: true); Assert.True(Start(sim));
        for (var i = 0; i < 96; i++) sim.Tick();
        Assert.Equal(8, sim.FloorOf(0)); Assert.Equal(16, sim.FloorOf(1)); Assert.Equal(24, sim.FloorOf(2));
    }
}
