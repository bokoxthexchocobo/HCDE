using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileFrictionExemptionTests
{
    [Theory]
    [InlineData(0, false, 4)]
    [InlineData(0, false, 0.03125)]
    [InlineData(64, true, 4)]
    [InlineData(64, true, 0.03125)]
    public void SharedPhysicsPreservesMissileMomentum(int z, bool fly, double speed)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 256 }] });
        var missile = new ProjectileActor(new Actor(), ProjectileKind.ImpBall)
        {
            SectorIndex = 0, Z = Fixed.FromInt(z), OnGround = z == 0, Fly = fly,
            VelocityX = Fixed.FromDouble(speed), VelocityY = Fixed.FromDouble(-speed)
        };
        ActorPhysics.Step(sim, missile);
        Assert.Equal(speed, missile.X.ToDouble()); Assert.Equal(-speed, missile.Y.ToDouble());
        Assert.Equal(speed, missile.VelocityX.ToDouble()); Assert.Equal(-speed, missile.VelocityY.ToDouble());
    }
}
