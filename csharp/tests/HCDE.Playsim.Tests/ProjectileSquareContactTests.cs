using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSquareContactTests
{
    [Theory]
    [InlineData(10, 10, 1, 0, true)]
    [InlineData(10, -10, 1, 0, true)]
    [InlineData(10, 12, 1, 0, false)]
    [InlineData(10, -12, 1, 0, false)]
    [InlineData(12, 10, 0, 1, false)]
    [InlineData(-12, 10, 0, 1, false)]
    [InlineData(40, 10, 60, 0, true)]
    [InlineData(40, 12, 60, 0, false)]
    public void TravelUsesStrictSquareActorBounds(int x, int y, int vx, int vy, bool contact)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(x, y, 3001); target.Brain = null; target.Radius = Fixed.FromInt(10);
        target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20); missile.Radius = Fixed.FromInt(2);
        missile.VelocityX = Fixed.FromInt(vx); missile.VelocityY = Fixed.FromInt(vy); missile.VelocityZ = default;
        missile.DamageExpression = _ => 7;
        missile.Tick();
        Assert.Equal(contact, missile.Destroyed);
        Assert.Equal(contact ? 993 : 1000, target.Health);
    }

    [Theory]
    [InlineData(-11, 33, -2, false, false)]
    [InlineData(13, 31, 2, false, false)]
    [InlineData(-10, 33, -2, false, true)]
    [InlineData(12, 31, 2, false, true)]
    [InlineData(-11, 33, -2, true, false)]
    [InlineData(13, 31, 2, true, false)]
    [InlineData(-10, 33, -2, true, true)]
    [InlineData(12, 31, 2, true, true)]
    public void HeightContactRequiresSimultaneousStrictHorizontalOverlap(int targetX, int z, int vz,
        bool canPass, bool contact)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(targetX, 0, 3001); target.Brain = null;
        target.Radius = Fixed.FromInt(10); target.Height = Fixed.FromInt(32); target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(z); missile.Radius = Fixed.FromInt(2);
        missile.CanPass = canPass; missile.VelocityX = Fixed.FromInt(2);
        missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(vz);
        missile.DamageExpression = _ => 7;
        missile.Tick();
        Assert.Equal(contact, missile.Destroyed);
        Assert.Equal(contact ? 993 : 1000, target.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DiagonalContactAppliesToRippingAndDontOverlap(bool rip)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(10, 10, 3001); target.Brain = null; target.Radius = Fixed.FromInt(10);
        target.Health = 1000; target.NoPain = true; target.DontOverlap = !rip;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(rip ? 20 : 64);
        missile.Radius = Fixed.FromInt(2); missile.CanPass = true; missile.DontOverlap = !rip; missile.Rip = rip;
        missile.VelocityX = Fixed.FromInt(1); missile.VelocityY = missile.VelocityZ = default;
        missile.DamageExpression = _ => 7;
        missile.Tick();
        Assert.Equal(!rip, missile.Destroyed);
        Assert.Equal(rip ? 993 : 1000, target.Health);
    }
}
