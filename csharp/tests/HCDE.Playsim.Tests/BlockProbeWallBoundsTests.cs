using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class BlockProbeWallBoundsTests
{
    [Theory]
    [InlineData(35, 0, 0, 35, true)]
    [InlineData(0, 35, 35, 0, true)]
    [InlineData(45, 0, 0, 45, false)]
    [InlineData(40, 0, 0, 40, true)]
    [InlineData(0, 40, 40, 0, false)]
    [InlineData(20, -100, 20, 100, false)]
    [InlineData(-100, -20, 100, -20, false)]
    [InlineData(19, -100, 19, 100, true)]
    public void ProbeUsesNativeBoxAndPreciseSideComparisons(int x1, int y1, int x2, int y2, bool blocked)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Sides = [new LevelSide { Sector = 0 }],
            Lines = [new LevelLine { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, SideFront = 0, SideBack = -1 }]
        });
        var actor = sim.AddBot(0, 0); actor.Radius = Fixed.FromInt(20);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2));
        Assert.Null(ActorJumpActions.CheckBlock(actor, 2, flags: 1));
        Assert.Equal(before, SimSavegame.Write(sim));
    }
}
