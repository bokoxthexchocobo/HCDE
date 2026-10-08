using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class VerticalMissileMovementTests
{
    [Theory]
    [InlineData(0, false, 0)]
    [InlineData(0, true, 0)]
    [InlineData(5, false, 0)]
    [InlineData(-1, false, 0)]
    [InlineData(5, false, 90)]
    [InlineData(5, false, 180)]
    [InlineData(5, false, 270)]
    [InlineData(5, false, 45)]
    public void VerticalMissilesUseDamagePresenceWithoutEvaluatingExpression(int damage, bool expression, double angle)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }] }, rngSeed: 42);
        var owner = sim.AddBot(-1000, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 100); missile.Angle = BamAngle.FromDegrees(angle);
        missile.VelocityX = missile.VelocityY = default;
        missile.VelocityZ = Fixed.FromInt(2); missile.Damage = damage;
        var calls = 0;
        if (expression) missile.DamageExpression = _ => { calls++; return 0; };
        var expected = new Actor { Angle = missile.Angle };
        if (damage != 0 || expression) expected.VelFromAngle(1.0 / Fixed.Unit);
        var random = sim.CombatRandomState;
        missile.Tick();
        Assert.False(missile.Destroyed);
        Assert.Equal(expected.VelocityX, missile.VelocityX);
        Assert.Equal(expected.VelocityY, missile.VelocityY);
        Assert.Equal(expected.VelocityX, missile.X);
        Assert.Equal(expected.VelocityY, missile.Y);
        Assert.Equal(102, missile.Z.ToDouble());
        Assert.Equal(2, missile.VelocityZ.ToDouble());
        Assert.Equal(174, missile.RemainingTics);
        Assert.Equal(0, calls);
        Assert.Equal(random, sim.CombatRandomState);
    }

    [Fact]
    public void ExistingHorizontalMotionIsPreserved()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 512 }] });
        var owner = sim.AddBot(-1000, 0); owner.Brain = null;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.SetXYZ(0, 0, 100); missile.Angle = BamAngle.FromDegrees(180);
        missile.VelocityX = new Fixed(1); missile.VelocityY = default;
        missile.VelocityZ = Fixed.FromInt(2);
        missile.Tick();
        Assert.Equal(1, missile.VelocityX.Raw);
        Assert.Equal(1, missile.X.Raw);
        Assert.Equal(0, missile.VelocityY.Raw);
    }
}
