using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorVelocityActionTests
{
    [Theory]
    [InlineData(0, 3, 4)]
    [InlineData(90, -4, 3)]
    [InlineData(180, -3, -4)]
    [InlineData(270, 4, -3)]
    public void RelativeAxesRotateBothComponents(int angle, int x, int y)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(angle) };
        ActorVelocityActions.ChangeVelocity(actor, 3, 4, 5, 3);
        Assert.Equal(x, actor.VelocityX.ToDouble()); Assert.Equal(y, actor.VelocityY.ToDouble());
        Assert.Equal(5, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(0, 4, 6, 8)]
    [InlineData(2, 3, 4, 5)]
    public void AbsoluteVelocityAddsOrReplaces(int flags, int x, int y, int z)
    {
        var actor = Moving(); ActorVelocityActions.ChangeVelocity(actor, 3, 4, 5, flags);
        Assert.Equal(x, actor.VelocityX.ToDouble()); Assert.Equal(y, actor.VelocityY.ToDouble());
        Assert.Equal(z, actor.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(0)]
    [InlineData(0.5)]
    public void ScaleSupportsReversalStoppingAndFractionalSpeeds(double scale)
    {
        var actor = Moving(); ActorVelocityActions.ScaleVelocity(actor, scale);
        Assert.Equal(scale, actor.VelocityX.ToDouble()); Assert.Equal(2 * scale, actor.VelocityY.ToDouble());
        Assert.Equal(3 * scale, actor.VelocityZ.ToDouble());
        ActorVelocityActions.Stop(actor);
        Assert.Equal(default, actor.VelocityX); Assert.Equal(default, actor.VelocityY); Assert.Equal(default, actor.VelocityZ);
    }

    [Fact]
    public void TargetFrameActionUsesTargetsAngleAndRoundTrips()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var caller = sim.AddBot(0, 0); var target = sim.AddBot(100, 0, thingId: 7);
        target.Angle = BamAngle.FromDegrees(90); caller.Brain!.SetTargetThingId(sim, 7);
        caller.States.Configure(caller, [new(-1, 0), new(-1, 0, Action: self =>
        {
            ActorVelocityActions.ChangeVelocity(self, 2, 0, 3, 3, AcsActorPointer.Target);
            ActorVelocityActions.ScaleVelocity(self, 2, AcsActorPointer.Target);
        })], 0);
        caller.States.Enter(caller, 1);
        Assert.Equal(4, target.VelocityY.ToDouble()); Assert.Equal(6, target.VelocityZ.ToDouble());
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        ActorVelocityActions.Stop(target); sim.RestoreState(state);
        Assert.Equal(4, target.VelocityY.ToDouble()); Assert.Equal(6, target.VelocityZ.ToDouble());
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void NullSelectionAndRejectedInputsLeaveVelocityUntouched()
    {
        var actor = Moving();
        ActorVelocityActions.ChangeVelocity(actor, 8, pointerSelector: AcsActorPointer.Null);
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorVelocityActions.ScaleVelocity(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorVelocityActions.ChangeVelocity(actor, z: double.PositiveInfinity));
        Assert.Throws<NotSupportedException>(() => ActorVelocityActions.ChangeVelocity(actor, flags: 4));
        Assert.Equal(1, actor.VelocityX.ToDouble()); Assert.Equal(2, actor.VelocityY.ToDouble());
        Assert.Equal(3, actor.VelocityZ.ToDouble());
    }

    private static Actor Moving() => new()
    { VelocityX = Fixed.FromInt(1), VelocityY = Fixed.FromInt(2), VelocityZ = Fixed.FromInt(3) };
}
