using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class SlideOpeningRuleTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(LevelLine.RailingFlag, false)]
    [InlineData(LevelLine.RailingFlag | LevelLine.BlockingFlag, true)]
    [InlineData(LevelLine.RailingFlag | LevelLine.BlockEverythingFlag, true)]
    public void SlideUsesUnraisedOpeningAndBlockingPriority(int flags, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, 128);
        var actor = sim.AddBot(-1, 80);
        var line = new LevelLine { SideFront = 0, SideBack = 1, Flags = flags };
        Assert.Equal(blocked, ActorPhysics.SlideLineBlocks(sim, actor, line));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(24, true)]
    public void SlideChecksObstructionAfterStepAndRestoresPose(short floor, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, 128);
        var actor = sim.AddBot(-1, 80);
        var overhead = sim.AddBot(-1, 80);
        overhead.Z = Fixed.FromInt(56);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked, ActorPhysics.SlideLineBlocks(sim, actor, sim.Level.Lines.Last()));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(true, false, 48, 0)]
    [InlineData(false, true, 0, 100)]
    public void SlideDoesNotApplyHuggerPositioning(bool floorHugger, bool ceilingHugger, short floor, int z)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, 96);
        var actor = sim.AddBot(-1, 80);
        actor.Z = Fixed.FromInt(z);
        actor.FloorHugger = floorHugger;
        actor.CeilingHugger = ceilingHugger;
        Assert.True(ActorPhysics.SlideLineBlocks(sim, actor, sim.Level.Lines.Last()));
        Assert.Equal(z, actor.Z.ToDouble());
    }
}
