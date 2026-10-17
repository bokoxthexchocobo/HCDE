using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileAimVectorTests
{
    [Theory]
    [InlineData(100)]
    [InlineData(-100)]
    public void VerticallyAlignedTargetGetsPureVerticalVelocity(int targetZ)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        target.Z = Fixed.FromInt(targetZ);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw);
        Assert.Equal(Math.Sign(targetZ) * missile.Speed, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void CoincidentSourceAndTargetProduceZeroVector()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        owner.Angle = BamAngle.FromDegrees(90);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        Assert.Equal(default, missile.VelocityX); Assert.Equal(default, missile.VelocityY);
        Assert.Equal(default, missile.VelocityZ); Assert.Equal(0u, missile.Angle.Raw);
    }

    [Theory]
    [InlineData(0.25, 0.5, 2)]
    [InlineData(-0.25, 0.5, -2)]
    [InlineData(0, 0.125, 1)]
    public void SubUnitHorizontalDistanceDoesNotFlattenTrajectory(double x, double y, double z)
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        target.SetXYZ(x, y, z);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.BaronBall, target);
        var scale = missile.Speed / Math.Sqrt(x * x + y * y + z * z);
        Assert.Equal(Fixed.FromDouble(x * scale), missile.VelocityX);
        Assert.Equal(Fixed.FromDouble(y * scale), missile.VelocityY);
        Assert.Equal(Fixed.FromDouble(z * scale), missile.VelocityZ);
        Assert.Equal(BamAngle.FromDegrees(Math.Atan2(y, x) * 180 / Math.PI), missile.Angle);
    }

    [Fact]
    public void ShortTargetCorrectionCanMakeCoincidentAimPointDown()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        target.Height = Fixed.FromInt(16);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw);
        Assert.Equal(-missile.Speed, missile.VelocityZ.ToDouble());
    }

    [Fact]
    public void RevenantUsesTemporarilyRaisedSourceForVerticalShot()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        target.Z = Fixed.FromInt(16);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.RevenantTracer, target);
        Assert.Equal(48, missile.Z.ToDouble()); Assert.Equal(0, missile.VelocityZ.Raw);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw);
        Assert.Equal(target.Id, missile.TracerId);
    }

    [Fact]
    public void VerticalAimSurvivesSaveRestore()
    {
        var sim = Room(); var owner = sim.AddBot(0, 0); var target = sim.AddBot(0, 0);
        target.Z = Fixed.FromInt(100);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.ImpBall, target); var saved = SimSavegame.Write(sim, true);
        missile.VelocityX = Fixed.FromInt(4); missile.VelocityZ = default;
        SimSavegame.Apply(sim, saved);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(missile.Speed, missile.VelocityZ.ToDouble());
        Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Things = [new LevelThing { Type = 1, X = 500 }], Sectors = [new LevelSector { CeilingHeight = 256 }] });
}
