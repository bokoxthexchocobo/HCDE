namespace HCDE.Playsim.Tests;

public class BlockProbeOpeningTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(24, true)]
    [InlineData(25, true)]
    public void AdjacentFloorControlsStepUpObstruction(short floor, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, 128);
        var actor = sim.AddBot(-1, 80);
        actor.AllowDropOff = true;
        var overhead = sim.AddBot(-1, 80);
        overhead.Z = Fixed.FromInt(56);
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32));
        Assert.Equal(before, SimSavegame.Write(sim));
    }

    [Theory]
    [InlineData(128, true)]
    [InlineData(80, false)]
    public void AdjacentCeilingControlsHuggerDropDistance(short ceiling, bool blocked)
    {
        var sim = GameplayFoundationTests.TwoRooms(0, ceiling);
        var actor = sim.AddBot(-1, 80);
        actor.CeilingHugger = true;
        actor.Height = Fixed.FromInt(56);
        actor.AllowDropOff = false;
        actor.Floating = false;
        var before = SimSavegame.Write(sim);
        Assert.Equal(blocked ? 2 : (int?)null, ActorJumpActions.CheckBlock(actor, 2, flags: 32));
        Assert.Equal(before, SimSavegame.Write(sim));
    }
}
