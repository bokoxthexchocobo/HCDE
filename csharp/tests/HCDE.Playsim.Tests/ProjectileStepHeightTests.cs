namespace HCDE.Playsim.Tests;

public class ProjectileStepHeightTests
{
    [Theory]
    [InlineData(1, 0, false)]
    [InlineData(24, 0, false)]
    [InlineData(24, 24, true)]
    [InlineData(24, 25, true)]
    [InlineData(0, 0, true)]
    [InlineData(-24, 0, true)]
    public void OrdinaryMissileCannotClimbDestinationFloor(short floor, int z, bool passes)
    {
        var sim = GameplayFoundationTests.TwoRooms(floor, 128);
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            X = Fixed.FromInt(-40), Y = Fixed.FromInt(80), Z = Fixed.FromInt(z), SectorIndex = 0
        };
        Assert.Equal(passes, ActorPhysics.TryMove(sim, missile, 40, 80, out _));
        Assert.Equal(passes ? 40 : -40, missile.X.ToDouble());
        Assert.Equal(z, missile.Z.ToDouble());
        Assert.Equal(passes ? 1 : 0, missile.SectorIndex);
    }

    [Fact]
    public void FloorHuggerRetainsSupportedStepExemption()
    {
        var sim = GameplayFoundationTests.TwoRooms(24, 128);
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            X = Fixed.FromInt(-40), Y = Fixed.FromInt(80), SectorIndex = 0, FloorHugger = true
        };
        Assert.True(ActorPhysics.TryMove(sim, missile, 40, 80, out _));
        Assert.Equal(24, missile.Z.ToDouble());
    }
}
