using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSightApertureIntegrationTests
{
    [Theory]
    [InlineData(20, 25, false)]
    [InlineData(30, 35, false)]
    [InlineData(0, 10, true)]
    public void ObservationActionsUseTargetHeightAperture(int floor, int ceiling, bool jumps)
    {
        var (sim, actor) = Room(); sim.Floors[1] = floor; sim.Ceilings[1] = ceiling;
        Assert.Equal(jumps ? 7 : (int?)null, ActorJumpActions.CheckSight(actor, 7));
        Assert.Equal(jumps ? 7 : (int?)null, ActorJumpActions.CheckSightOrRange(actor, 1, 7));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ObservationIgnoresActorVisibilityWithoutRandomDraw(bool invisible)
    {
        var (sim, actor) = Room(); actor.Invisible = invisible; actor.MVisBlocked = !invisible;
        Assert.Null(ActorJumpActions.CheckSight(actor, 7));
        Assert.Null(ActorJumpActions.CheckSightOrRange(actor, 1, 7));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void TargetLosObserverDirectionUsesCorrectEyeHeight()
    {
        var (sim, actor) = Room(); actor.Height = Fixed.FromInt(128);
        sim.Floors[1] = 45; sim.Ceilings[1] = 47;
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(actor, 7));
        Assert.Equal(7, ActorJumpActions.JumpIfTargetInLOS(actor, 7, flags: 128));
        Assert.Equal(7, ActorJumpActions.JumpIfInTargetLOS(actor, 7));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TargetLosActionsIgnoreVisibilityWithoutRandomDraw(bool reverse)
    {
        var (sim, actor) = Room(); actor.Invisible = actor.MVisBlocked = true;
        var target = sim.Players.Single(); target.Invisible = target.MVisBlocked = true;
        Assert.Equal(7, reverse ? ActorJumpActions.JumpIfInTargetLOS(actor, 7)
            : ActorJumpActions.JumpIfTargetInLOS(actor, 7));
        Assert.Null(sim.CaptureState().CheckSightRandomState);
    }

    [Fact]
    public void NoSightFlagStillBypassesClosedOpening()
    {
        var (sim, actor) = Room(); sim.Ceilings[1] = sim.Floors[1];
        Assert.Null(ActorJumpActions.JumpIfTargetInLOS(actor, 7));
        Assert.Null(ActorJumpActions.JumpIfInTargetLOS(actor, 7));
        Assert.Equal(7, ActorJumpActions.JumpIfTargetInLOS(actor, 7, flags: 2));
        Assert.Equal(7, ActorJumpActions.JumpIfInTargetLOS(actor, 7, flags: 2));
    }

    [Fact]
    public void WithinRangeSkipsBlockedObservationSight()
    {
        var (sim, actor) = Room(); sim.Ceilings[1] = sim.Floors[1];
        Assert.Null(ActorJumpActions.CheckSightOrRange(actor, 128, 7));
        Assert.Equal(7, ActorJumpActions.CheckSightOrRange(actor, 127, 7));
    }

    private static (AuthoritySimulation, Actor) Room()
    {
        var geometry = GameplayFoundationTests.TwoRooms(0, 128).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = geometry.Sectors, Sides = geometry.Sides,
            Lines = geometry.Lines, Things = [new LevelThing { Type = 1, X = -64 }] });
        var actor = sim.AddBot(64, 0); actor.SetTargetPointer(sim.Players.Single());
        return (sim, actor);
    }
}
