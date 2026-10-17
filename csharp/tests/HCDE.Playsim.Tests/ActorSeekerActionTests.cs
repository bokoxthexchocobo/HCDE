using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorSeekerActionTests
{
    [Theory]
    [InlineData(100, 10, 90)]
    [InlineData(10, 20, 20)]
    [InlineData(10, 60, 45)]
    [InlineData(90, 0, 90)]
    public void ThresholdControlsHalfAngleAndTurnCap(double threshold, double cap, double expected)
    {
        var (_, missile, _) = Room();
        Assert.True(ActorSeekerActions.Seek(missile, threshold, cap));
        Assert.Equal(expected, missile.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(-100, 0, 340)]
    [InlineData(0, -100, 340)]
    public void NegativeDeltaAndOppositeDirectionTurnCounterclockwise(int x, int y, double expected)
    {
        var (_, missile, target) = Room(); target.SetXYZ(x, y, 0);
        Assert.True(ActorSeekerActions.Seek(missile, 0, 20)); Assert.Equal(expected, missile.Angle.ToDegrees(), 5);
    }

    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 13)]
    public void SpeedModeUsesClassSpeedOrFullCurrentVelocity(bool current, int expected)
    {
        var (_, missile, _) = Room(); missile.VelocityX = Fixed.FromInt(3); missile.VelocityY = Fixed.FromInt(4);
        missile.VelocityZ = Fixed.FromInt(12);
        Assert.True(ActorSeekerActions.Seek(missile, 100, 0, current));
        Assert.InRange(Math.Abs(missile.VelXYToSpeed() - expected), 0, 1.0 / 65536);
        Assert.Equal(12, missile.VelocityZ.ToDouble());
    }

    [Theory]
    [InlineData(8, false)]
    [InlineData(9, true)]
    [InlineData(-56, false)]
    [InlineData(-57, true)]
    public void VerticalSteeringRequiresStrictSeparation(int targetZ, bool separated)
    {
        var (_, missile, target) = Room(); target.Z = Fixed.FromInt(targetZ); missile.VelocityZ = Fixed.FromInt(2);
        ActorSeekerActions.Seek(missile, 100, 0);
        var expected = separated ? Fixed.FromDouble((target.Center() - missile.Center()) / 10) : Fixed.FromInt(2);
        Assert.Equal(expected, missile.VelocityZ);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HuggersKeepVerticalVelocity(bool ceiling)
    {
        var (_, missile, target) = Room(); target.Z = Fixed.FromInt(100);
        missile.FloorHugger = !ceiling; missile.CeilingHugger = ceiling; missile.VelocityZ = Fixed.FromInt(2);
        Assert.True(ActorSeekerActions.Seek(missile, 100, 0)); Assert.Equal(2, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void UnseekableTargetRetainsTracerButNonshootableTargetClearsIt()
    {
        var (_, missile, target) = Room(); target.CantSeek = true;
        Assert.False(ActorSeekerActions.Seek(missile, 0, 20)); Assert.Equal(target.Id, missile.TracerId);
        target.CantSeek = false; target.Shootable = false;
        Assert.False(ActorSeekerActions.Seek(missile, 0, 20)); Assert.Null(missile.TracerId);
        Assert.False(ActorSeekerActions.Seek(missile, 0, 20));
    }

    [Fact]
    public void ZeroSpeedReturnsSuccessWithoutChangingMotionOrYaw()
    {
        var (_, missile, _) = Room(); missile.MovementSpeed = default;
        missile.VelocityZ = Fixed.FromInt(2); var motion = (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ);
        Assert.True(ActorSeekerActions.Seek(missile, 0, 20));
        Assert.Equal(motion, (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ));
    }

    [Fact]
    public void ContinuedSteeringMatchesAfterSaveRestore()
    {
        var (sim, missile, target) = Room(); target.Z = Fixed.FromInt(100);
        ActorSeekerActions.Seek(missile, 0, 20); var saved = SimSavegame.Write(sim, true);
        ActorSeekerActions.Seek(missile, 0, 20); var expected = (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ);
        SimSavegame.Apply(sim, saved); ActorSeekerActions.Seek(missile, 0, 20);
        Assert.Equal(expected, (missile.Angle, missile.VelocityX, missile.VelocityY, missile.VelocityZ));
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Things = [new LevelThing { Type = 1, X = 500 }],
            Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 100);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall);
        missile.SetXYZ(0, 0, 0); missile.Angle = default; missile.TracerId = target.Id;
        return (sim, missile, target);
    }
}
