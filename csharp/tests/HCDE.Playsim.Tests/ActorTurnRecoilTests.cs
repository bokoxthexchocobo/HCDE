using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorTurnRecoilTests
{
    [Theory]
    [InlineData(0, 5, -5, 0)]
    [InlineData(90, 5, 0, -5)]
    [InlineData(180, 5, 5, 0)]
    [InlineData(270, 5, 0, 5)]
    [InlineData(0, -5, 5, 0)]
    public void RecoilAddsOppositeFacingImpulseWithoutReplacingVelocity(int yaw, int speed, int x, int y)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(yaw), VelocityX = Fixed.FromInt(3),
            VelocityY = Fixed.FromInt(2), VelocityZ = Fixed.FromInt(7) };
        ActorPropertyActions.Recoil(actor, speed);
        Assert.Equal(x + 3, actor.VelocityX.ToDouble()); Assert.Equal(y + 2, actor.VelocityY.ToDouble());
        Assert.Equal(7, actor.VelocityZ.ToDouble()); Assert.Equal(BamAngle.FromDegrees(yaw), actor.Angle);
    }

    [Theory]
    [InlineData(350, 20, 10)]
    [InlineData(10, -20, 350)]
    [InlineData(45, 720, 45)]
    public void TurnWrapsBothDirections(int yaw, int turn, int expected)
    {
        var actor = new Actor { Angle = BamAngle.FromDegrees(yaw) };
        ActorPropertyActions.Turn(actor, turn);
        Assert.InRange(Math.Abs(actor.Angle.ToDegrees() - expected), 0, 0.000001);
    }

    [Fact]
    public void FrameTurnControlsFollowingRecoilAndBothRoundTrip()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var actor = sim.AddBot(0, 0);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: self =>
        { ActorPropertyActions.Turn(self, 90); ActorPropertyActions.Recoil(self, 5); })], 0);
        actor.States.Enter(actor, 1);
        Assert.Equal(-5, actor.VelocityY.ToDouble()); Assert.Equal(90, actor.Angle.ToDegrees());
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.Angle = default; actor.VelocityY = default; sim.RestoreState(state);
        Assert.Equal(-5, actor.VelocityY.ToDouble()); Assert.Equal(90, actor.Angle.ToDegrees());
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    [Fact]
    public void NonfiniteInputsFailBeforeChangingActor()
    {
        var actor = new Actor { VelocityX = Fixed.FromInt(3) };
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.Turn(actor, double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => ActorPropertyActions.Recoil(actor, double.PositiveInfinity));
        Assert.Equal(0u, actor.Angle.Raw); Assert.Equal(3, actor.VelocityX.ToDouble());
    }
}
