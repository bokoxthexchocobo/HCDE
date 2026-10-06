using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorStopSpeedTests
{
    [Theory]
    [InlineData(0.03125, 0.03125, true)]
    [InlineData(0.0625, 0.03125, false)]
    [InlineData(0.03125, -1, false)]
    public void GroundStopRequiresBothAxesBelowThresholdBeforeDamping(double x, double y, bool stops)
    {
        var sim = Room(); var actor = new Actor { SectorIndex = 0, OnGround = true,
            VelocityX = Fixed.FromDouble(x), VelocityY = Fixed.FromDouble(y) };
        ActorPhysics.Step(sim, actor);
        Assert.Equal(x, actor.X.ToDouble()); Assert.Equal(y, actor.Y.ToDouble());
        Assert.Equal(Fixed.FromDouble(stops ? 0 : x * ActorPhysics.GroundFriction), actor.VelocityX);
        Assert.Equal(Fixed.FromDouble(stops ? 0 : y * ActorPhysics.GroundFriction), actor.VelocityY);
    }

    [Fact]
    public void OrdinaryAirborneActorDoesNotUseGroundStopCutoff()
    {
        var sim = Room(); var actor = new Actor { SectorIndex = 0, Z = Fixed.FromInt(64), NoGravity = true,
            VelocityX = Fixed.FromDouble(0.03125) };
        ActorPhysics.Step(sim, actor); Assert.Equal(0.03125, actor.VelocityX.ToDouble());
    }

    [Fact]
    public void ActiveFlightUsesCoupledStopRule()
    {
        var sim = Room(); var actor = new Actor { SectorIndex = 0, Z = Fixed.FromInt(64), NoGravity = true, Fly = true,
            VelocityX = Fixed.FromDouble(0.0625), VelocityY = Fixed.FromDouble(0.03125) };
        ActorPhysics.Step(sim, actor);
        Assert.Equal(Fixed.FromDouble(0.0625 * ActorPhysics.FlyingFriction), actor.VelocityX);
        Assert.Equal(Fixed.FromDouble(0.03125 * ActorPhysics.FlyingFriction), actor.VelocityY);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
