using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileExplosionCleanupTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FloorAndSkyImpactsClearMotionAndShootability(bool sky)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = sky ? "F_SKY1" : "FLOOR0_1" }] });
        var missile = sim.SpawnProjectile(sim.AddBot(-200, 0), ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(1); missile.Shootable = true;
        missile.VelocityX = Fixed.FromInt(3); missile.VelocityY = Fixed.FromInt(4); missile.VelocityZ = Fixed.FromInt(-10);
        missile.Tick();
        Assert.True(missile.Destroyed); Assert.False(missile.Shootable);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw); Assert.Equal(0, missile.VelocityZ.Raw);
    }

    [Fact]
    public void DirectDamageSeesIncomingMotionBeforeExplosionCleanup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20); missile.Shootable = true;
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = Fixed.FromInt(1); missile.VelocityZ = Fixed.FromInt(1);
        var calls = 0;
        missile.DamageExpression = actor =>
        {
            Assert.False(actor.Destroyed);
            Assert.Equal(2, actor.VelocityX.ToDouble()); Assert.Equal(1, actor.VelocityY.ToDouble());
            Assert.Equal(1, actor.VelocityZ.ToDouble()); Assert.True(actor.Shootable); calls++; return 7;
        };
        missile.Tick();
        Assert.Equal(1, calls); Assert.Equal(993, target.Health);
        Assert.True(missile.Destroyed); Assert.False(missile.Shootable);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw); Assert.Equal(0, missile.VelocityZ.Raw);
    }
    [Theory]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.Rocket)]
    public void DamageCallbackRemovalSuppressesSecondExplosion(ProjectileKind kind)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var nearby = sim.AddBot(0, 60, 3001); nearby.Brain = null; nearby.Health = 1000; nearby.NoPain = true;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        var calls = 0;
        missile.DamageExpression = actor =>
        { Assert.False(actor.Destroyed); calls++; actor.Destroy(); return 7; };
        sim.DrainSoundRequests();
        missile.Tick();
        Assert.Equal(1, calls); Assert.True(missile.Destroyed);
        Assert.Equal(993, target.Health); Assert.Equal(1000, nearby.Health);
        Assert.Empty(sim.DrainSoundRequests());
    }
    [Theory]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Bfg)]
    public void ExplosionEffectsPrecedeFinalRemovalNotification(ProjectileKind kind)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3004); owner.Brain = null;
        var target = sim.AddBot(40, 0, 3001); target.Brain = null; target.Health = 10000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, kind);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(1);
        missile.VelocityX = Fixed.FromInt(1); missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(-10);
        var notifications = 0;
        sim.ActorDestroyed += actor =>
        {
            if (!ReferenceEquals(actor, missile)) return;
            Assert.True(target.Health < 10000);
            Assert.False(missile.Shootable); Assert.Equal(0, missile.VelocityX.Raw);
            notifications++;
        };
        missile.Tick();
        Assert.Equal(1, notifications); Assert.True(missile.Destroyed); Assert.True(target.Health < 10000);
    }
}
