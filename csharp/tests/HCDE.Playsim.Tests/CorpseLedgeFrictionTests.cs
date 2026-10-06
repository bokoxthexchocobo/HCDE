namespace HCDE.Playsim.Tests;

public class CorpseLedgeFrictionTests
{
    [Theory]
    [InlineData(true, 0.5, 0, true)]
    [InlineData(true, 0, 0.5, true)]
    [InlineData(true, 0.25, 0.25, false)]
    [InlineData(false, 0.5, 0, false)]
    public void PartlySupportedCorpseSlidesOnlyAboveStrictSpeedThreshold(bool dead, double xSpeed, double ySpeed, bool slides)
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128);
        var actor = sim.AddBot(1, 80); actor.Brain = null; actor.AllowDropOff = true;
        actor.Z = default; actor.SectorIndex = 1; actor.OnGround = true;
        if (dead) actor.Health = 0;
        actor.VelocityX = Fixed.FromDouble(xSpeed); actor.VelocityY = Fixed.FromDouble(ySpeed);
        ActorPhysics.Step(sim, actor);
        Assert.True(actor.OnGround); Assert.Equal(0, actor.Z.Raw);
        Assert.Equal(Fixed.FromDouble(xSpeed * (slides ? 1 : ActorPhysics.GroundFriction)), actor.VelocityX);
        Assert.Equal(Fixed.FromDouble(ySpeed * (slides ? 1 : ActorPhysics.GroundFriction)), actor.VelocityY);
    }

    [Fact]
    public void CorpseOnItsOwnSectorFloorStillReceivesFriction()
    {
        var sim = GameplayFoundationTests.TwoRooms(-24, 128);
        var actor = sim.AddBot(-40, 80); actor.Brain = null; actor.Health = 0;
        actor.VelocityX = Fixed.FromInt(1);
        ActorPhysics.Step(sim, actor);
        Assert.Equal(ActorPhysics.GroundFriction, actor.VelocityX.ToDouble());
    }
}
