using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ChaseDirectionTests
{
    [Theory]
    [InlineData(100, 100, 1)]
    [InlineData(-100, 100, 3)]
    [InlineData(-100, -100, 5)]
    [InlineData(100, -100, 7)]
    public void DirectDiagonalConsumesOnlyTryWalkStream(int dx, int dy, byte direction)
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.MoveDirection = 8;
        var expected = Room(); var count = expected.NextTryWalkByte() & 15;
        ActorWanderActions.ChooseChaseDirection(actor, dx, dy);
        Assert.Equal(direction, actor.MoveDirection); Assert.Equal(count, actor.MoveCount);
        Assert.Equal(Fixed.FromDouble(Math.Sign(dx) * 8 * Math.Sqrt(0.5)), actor.X);
        Assert.Equal(Fixed.FromDouble(Math.Sign(dy) * 8 * Math.Sqrt(0.5)), actor.Y);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
        Assert.Equal(expected.CaptureState().TryWalkRandomState, sim.CaptureState().TryWalkRandomState);
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(-10, 0)]
    [InlineData(10.01, 2)]
    [InlineData(-10.01, 6)]
    public void CardinalThresholdIsStrict(double dy, byte expectedDirection)
    {
        var sim = Room(); var actor = sim.Actors.Single();
        ActorWanderActions.ChooseChaseDirection(actor, 0, dy);
        Assert.Equal(expectedDirection, actor.MoveDirection);
    }

    [Fact]
    public void AvoidanceCanUseImmediateTurnaroundWithoutOrderingRandomDraw()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.MoveDirection = 4;
        ActorWanderActions.ChooseChaseDirection(actor, 100, 0, avoidingDropOff: true);
        Assert.Equal(0, actor.MoveDirection); Assert.Equal(Fixed.FromInt(8), actor.X);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
    }

    [Fact]
    public void ExhaustedSearchSetsNoDirectionWithoutTryWalkDraw()
    {
        var sim = Room(); var actor = sim.Actors.Single(); actor.Z = Fixed.FromInt(16); actor.MoveCount = 23;
        ActorWanderActions.ChooseChaseDirection(actor, 100, 100);
        Assert.Equal(8, actor.MoveDirection); Assert.Equal(23, actor.MoveCount);
        Assert.Equal(default, actor.X); Assert.Equal(default, actor.Y);
        Assert.Null(sim.CaptureState().TryWalkRandomState);
        var expected = Room(); expected.NextNewChaseDirByte(); expected.NextNewChaseDirByte();
        Assert.Equal(expected.CaptureState().NewChaseDirRandomState, sim.CaptureState().NewChaseDirRandomState);
    }

    [Fact]
    public void SavedDirectionSearchContinuesWithSamePositionCountsAndStreams()
    {
        var sim = Room(); var actor = sim.Actors.Single();
        ActorWanderActions.ChooseChaseDirection(actor, 100, 0);
        var saved = SimSavegame.Write(sim, true);
        ActorWanderActions.ChooseChaseDirection(actor, -100, 100);
        var result = SimSavegame.Write(sim, true);
        SimSavegame.Apply(sim, saved);
        ActorWanderActions.ChooseChaseDirection(actor, -100, 100);
        Assert.Equal(result, SimSavegame.Write(sim, true));
    }

    [Theory]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.PositiveInfinity)]
    public void InvalidVectorRejectsBeforeConsumingRandom(double dx, double dy)
    {
        var sim = Room(); var before = SimSavegame.Write(sim, true);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorWanderActions.ChooseChaseDirection(sim.Actors.Single(), dx, dy));
        Assert.Equal(before, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 3004 }]
        });
        sim.Actors.Single().MovementSpeed = Fixed.FromInt(8);
        return sim;
    }
}
