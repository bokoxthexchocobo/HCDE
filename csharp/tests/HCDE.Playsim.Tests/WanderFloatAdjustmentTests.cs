using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class WanderFloatAdjustmentTests
{
    [Theory]
    [InlineData(4, 4)]
    [InlineData(0, 0)]
    [InlineData(-4, -4)]
    public void TallStepUsesSignedFloatSpeedAndKeepsPartialMovement(int speed, int expected)
    {
        var sim = Room(64, 128); var actor = sim.Actors.Single(); actor.FloatSpeed = speed;
        Walk(actor);
        Assert.Equal(expected, actor.Z.ToDouble()); Assert.True(actor.InFloat);
        Assert.InRange(actor.X.ToDouble(), -39, -20);
        Assert.Null(sim.CaptureState().NewChaseDirRandomState);
    }

    [Fact]
    public void LowCeilingLowersFloatingActor()
    {
        var sim = Room(0, 64); var actor = sim.Actors.Single(); actor.Z = Fixed.FromInt(32);
        Walk(actor); Assert.Equal(28, actor.Z.ToDouble()); Assert.True(actor.InFloat);
    }

    [Fact]
    public void ClosedOpeningDoesNotPermitFloatCorrection()
    {
        var sim = Room(64, 100); var actor = sim.Actors.Single();
        Walk(actor); Assert.Equal(0, actor.Z.ToDouble()); Assert.False(actor.InFloat);
    }

    [Fact]
    public void SolidActorDoesNotPermitFloatCorrection()
    {
        var sim = Room(64, 128); var actor = sim.Actors.Single();
        sim.AddBot(-8, 0);
        Walk(actor); Assert.Equal(0, actor.Z.ToDouble()); Assert.False(actor.InFloat);
    }

    [Fact]
    public void OrdinaryMoveStillRejectsTallStepWithoutChangingHeight()
    {
        var sim = Room(64, 128); var actor = sim.Actors.Single();
        Assert.False(ActorPhysics.TryMove(sim, actor, -10, 0, out _));
        Assert.Equal(0, actor.Z.ToDouble()); Assert.Equal(-40, actor.X.ToDouble());
    }

    private static void Walk(Actor actor)
    {
        actor.ReactionTime = 0; actor.MoveCount = 10; actor.MovementSpeed = Fixed.FromInt(80);
        ActorWanderActions.Wander(actor, ActorWanderFlags.NoRandomTurn | ActorWanderFlags.StopIfBlocked);
    }

    private static AuthoritySimulation Room(short floor, short ceiling)
    {
        var geometry = GameplayFoundationTests.TwoRooms(floor, ceiling).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = geometry.Sectors, Lines = geometry.Lines, Sides = geometry.Sides,
            Things = [new LevelThing { Type = 3004, X = -40 }]
        });
        var actor = sim.Actors.Single(); actor.Floating = true; actor.NoGravity = true;
        return sim;
    }
}
