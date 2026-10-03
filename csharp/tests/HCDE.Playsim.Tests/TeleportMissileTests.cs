using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class TeleportMissileTests
{
    [Theory]
    [InlineData(128, 96)]
    [InlineData(80, 72)]
    public void MissileTeleportPreservesHeightAndVerticalVelocityAndRedirectsHorizontalSpeed(short ceiling, int expectedZ)
    {
        var geometry = GameplayFoundationTests.TwoRooms(64, ceiling).Level;
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Sectors = geometry.Sectors, Sides = geometry.Sides, Lines = geometry.Lines,
            Things = [new LevelThing { Type = LineSpecials.TeleportDestType, X = 40, Angle = 90 }],
        });
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            X = Fixed.FromInt(-40), Z = Fixed.FromInt(32), SectorIndex = 0,
            VelocityX = Fixed.FromInt(3), VelocityY = Fixed.FromInt(4), VelocityZ = Fixed.FromDouble(2.5),
            ReactionTime = -5,
            NoTeleport = false,
        };
        Assert.True(LineSpecials.Execute(sim, missile, LineSpecials.Teleport, 0));
        Assert.Equal(expectedZ, missile.Z.ToDouble());
        Assert.Equal(0, missile.VelocityX.ToDouble());
        Assert.Equal(5, missile.VelocityY.ToDouble());
        Assert.Equal(2.5, missile.VelocityZ.ToDouble());
        Assert.Equal(-5, missile.ReactionTime);
        Assert.Equal(1, missile.SectorIndex);
        Assert.False(missile.OnGround);
    }

    [Fact]
    public void FailedMissileTeleportPreservesVelocity()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel());
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            VelocityX = Fixed.FromInt(3), VelocityY = Fixed.FromInt(4), VelocityZ = Fixed.FromInt(2),
            NoTeleport = false,
        };
        Assert.False(LineSpecials.Execute(sim, missile, LineSpecials.Teleport, 0));
        Assert.Equal(3, missile.VelocityX.ToDouble());
        Assert.Equal(4, missile.VelocityY.ToDouble());
        Assert.Equal(2, missile.VelocityZ.ToDouble());
    }
}
