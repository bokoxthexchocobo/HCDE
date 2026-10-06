using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFloorClipTests
{
    [Theory]
    [InlineData(0, 10, false, 10)]
    [InlineData(50, 10, false, 60)]
    [InlineData(56, 10, true, 56)]
    [InlineData(0, -2, false, -2)]
    public void SinkUsesPreStepCompletionWithoutClamping(double initial, double speed, bool complete, double expected)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004); actor.FloorClip = initial;
        Assert.Equal(complete, ActorPropertyActions.SinkMobj(actor, speed));
        Assert.Equal(expected, actor.FloorClip); RoundTrip(sim, actor);
    }

    [Theory]
    [InlineData(10, 2, false, 8)]
    [InlineData(10, 20, true, 0)]
    [InlineData(0, 2, true, 0)]
    [InlineData(-2, 2, true, -2)]
    [InlineData(10, -2, false, 12)]
    public void RaiseClampsOnlyWhenCrossingZero(double initial, double speed, bool complete, double expected)
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004); actor.FloorClip = initial;
        Assert.Equal(complete, ActorPropertyActions.RaiseMobj(actor, speed));
        Assert.Equal(expected, actor.FloorClip); RoundTrip(sim, actor);
    }

    [Fact]
    public void LegacyAbsenceClearsValueAndNonfiniteStateIsRejected()
    {
        var sim = Room(); var actor = sim.AddBot(200, 0, 3004); var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.FloorClip = 3; sim.RestoreState(state); Assert.Equal(0, actor.FloorClip);
        state.Actors.Single(item => item.Id == actor.Id).FloorClip = double.NaN;
        Assert.Throws<InvalidOperationException>(() => sim.RestoreState(state));
        Assert.Equal(0, actor.FloorClip);
    }

    [Fact]
    public void DeprecatedFacingFlagHasNoEffect()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0); var target = sim.AddBot(0, 100);
        ActorOrientationActions.Face(actor, target, flags: 8);
        Assert.Equal(90, actor.Angle.ToDegrees(), 5);
    }

    private static void RoundTrip(AuthoritySimulation sim, Actor actor)
    {
        var value = actor.FloorClip; var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.FloorClip = 99; sim.RestoreState(state); Assert.Equal(value, actor.FloorClip);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
