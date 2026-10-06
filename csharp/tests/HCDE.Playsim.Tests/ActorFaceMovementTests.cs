using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorFaceMovementTests
{
    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(0, 1, 90)]
    [InlineData(-1, 0, 180)]
    [InlineData(0, -1, 270)]
    public void FacesHorizontalVelocity(double x, double y, double expected)
    {
        var actor = Moving(x, y, 0);
        Assert.True(ActorOrientationActions.FaceMovementDirection(actor));
        Assert.Equal(expected, actor.Angle.ToDegrees()); Assert.Equal(0, actor.PitchDegrees);
    }

    [Theory]
    [InlineData(1, -45)]
    [InlineData(-1, 45)]
    public void FacesVerticalVelocity(double z, double expected)
    {
        var actor = Moving(1, 0, z); ActorOrientationActions.FaceMovementDirection(actor);
        Assert.Equal(expected, actor.PitchDegrees);
    }

    [Theory]
    [InlineData(350, 0, 5, 2, 357)]
    [InlineData(10, 0, 5, 2, 3)]
    [InlineData(350, 0, 15, 2, 2)]
    [InlineData(0, 90, 0, 10, 100)]
    public void YawLimitsFollowShortestTurnAndNativeOffset(double current, double desired, double limit,
        double offset, double expected)
    {
        var radians = desired * Math.PI / 180;
        var actor = Moving(Math.Cos(radians), Math.Sin(radians), 0); actor.Angle = BamAngle.FromDegrees(current);
        ActorOrientationActions.FaceMovementDirection(actor, offset, limit, flags: 1);
        Assert.Equal(expected, actor.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(-1, -10)]
    public void LimitedPitchPreservesNativeSubtractionDirection(double z, double expected)
    {
        var actor = Moving(1, 0, z);
        ActorOrientationActions.FaceMovementDirection(actor, pitchLimit: 10, flags: 4);
        Assert.Equal(expected, actor.PitchDegrees);
    }

    [Fact]
    public void StationaryYawIsPreservedAndPitchResets()
    {
        var actor = Moving(0, 0, 0); actor.Angle = BamAngle.FromDegrees(90); actor.PitchDegrees = 30;
        Assert.True(ActorOrientationActions.FaceMovementDirection(actor, offset: 10));
        Assert.Equal(90, actor.Angle.ToDegrees()); Assert.Equal(0, actor.PitchDegrees);
    }

    [Fact]
    public void SuppressionFlagsAndNullSelectionPreserveOrientation()
    {
        var actor = Moving(0, 1, 1); actor.PitchDegrees = 30;
        Assert.False(ActorOrientationActions.FaceMovementDirection(actor, flags: 5));
        Assert.False(ActorOrientationActions.FaceMovementDirection(actor, pointerSelector: AcsActorPointer.Null));
        Assert.Equal(0u, actor.Angle.Raw); Assert.Equal(30, actor.PitchDegrees);
        ActorOrientationActions.FaceMovementDirection(actor, flags: 1);
        Assert.Equal(90, actor.Angle.ToDegrees()); Assert.Equal(30, actor.PitchDegrees);
    }

    [Fact]
    public void FrameFacesSelectedTargetAndSaveRestoresOrientation()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        caller.Brain!.SetTargetThingId(sim, 7); target.VelocityY = Fixed.FromInt(1); target.VelocityZ = Fixed.FromInt(1);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
            ActorOrientationActions.FaceMovementDirection(self, pointerSelector: AcsActorPointer.Target))], 0);
        caller.States.Enter(caller, 1);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        target.Angle = default; target.PitchDegrees = 0; sim.RestoreState(state);
        Assert.Equal(90, target.Angle.ToDegrees()); Assert.Equal(-45, target.PitchDegrees);
        Assert.Equal(0u, caller.Angle.Raw); Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void InvalidParametersFailBeforeMutation()
    {
        var actor = Moving(0, 1, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.FaceMovementDirection(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.FaceMovementDirection(actor, angleLimit: double.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorOrientationActions.FaceMovementDirection(actor, pitchLimit: double.NaN));
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.FaceMovementDirection(actor, flags: 8));
        var player = new PlayerPawn();
        Assert.Throws<NotSupportedException>(() => ActorOrientationActions.FaceMovementDirection(player, flags: 2));
        Assert.False(ActorOrientationActions.FaceMovementDirection(player, flags: 7));
        Assert.Equal(0u, actor.Angle.Raw); Assert.Equal(0, actor.PitchDegrees);
    }

    private static Actor Moving(double x, double y, double z) => new()
    { VelocityX = Fixed.FromDouble(x), VelocityY = Fixed.FromDouble(y), VelocityZ = Fixed.FromDouble(z) };
}
