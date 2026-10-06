using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSignedTicsTests
{
    [Theory]
    [InlineData(-2)]
    [InlineData(-100)]
    [InlineData(int.MinValue + 1)]
    public void NegativeAssignedTicsAdvanceOnNextTick(int tics)
    {
        var actor = new Actor();
        actor.States.Configure(actor, [new(-1, 0), new(10, 2, Action: self => self.States.SetTics(tics)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1); Assert.Equal(1, actor.States.Current);
        Assert.Equal(tics, actor.States.RemainingTics);
        actor.States.Tick(actor); Assert.Equal(2, actor.States.Current);
    }

    [Theory]
    [InlineData(-1, -1, 1)]
    [InlineData(int.MinValue, int.MaxValue, 1)]
    [InlineData(int.MaxValue, int.MaxValue - 1, 1)]
    [InlineData(0, -1, 2)]
    public void DirectTimerHandlesHoldZeroAndIntegerBoundaries(int initial, int expectedTics, int expectedState)
    {
        var actor = new Actor(); actor.States.Configure(actor, [new(-1, 0), new(10, 2), new(-1, 2)], 1);
        actor.States.SetTics(initial); actor.States.Tick(actor);
        Assert.Equal(expectedTics, actor.States.RemainingTics); Assert.Equal(expectedState, actor.States.Current);
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(int.MinValue)]
    public void SignedTimerRestoresAndRetainsExactSaveBytes(int tics)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0); actor.States.SetTics(tics);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.States.SetTics(42); sim.RestoreState(state);
        Assert.Equal(tics, actor.States.RemainingTics); Assert.Equal(bytes, SimSavegame.Write(sim));
    }
}
