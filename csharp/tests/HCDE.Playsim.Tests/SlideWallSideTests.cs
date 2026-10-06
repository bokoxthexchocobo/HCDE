using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SlideWallSideTests
{
    [Theory]
    [InlineData(-1, false, false, false)]
    [InlineData(1, false, false, true)]
    [InlineData(-1, true, false, true)]
    [InlineData(1, true, false, false)]
    [InlineData(0, false, false, true)]
    [InlineData(0, false, true, false)]
    public void OneSidedSlideUsesNativeSideClassifier(int x, bool reversed, bool compatSide, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128);
        var actor = sim.AddBot(x, 80);
        var line = new LevelLine
        {
            X1 = 0, Y1 = reversed ? 128 : -128, X2 = 0, Y2 = reversed ? -128 : 128,
            SideFront = 0, SideBack = -1, Flags = compatSide ? LevelLine.CompatSideFlag : 0
        };
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked, ActorPhysics.SlideLineBlocks(sim, actor, line));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1)]
    public void TwoSidedBlockingLineStillBlocksFromBothSides(int x)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128);
        var actor = sim.AddBot(x, 80);
        var line = new LevelLine
        {
            X1 = 0, Y1 = -128, X2 = 0, Y2 = 128,
            SideFront = 0, SideBack = 1, Flags = LevelLine.BlockingFlag
        };
        Assert.True(ActorPhysics.SlideLineBlocks(sim, actor, line));
    }
}
