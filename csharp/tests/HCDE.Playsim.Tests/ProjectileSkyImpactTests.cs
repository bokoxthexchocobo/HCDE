using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileSkyImpactTests
{
    [Theory]
    [InlineData(ProjectileKind.Rocket, false, true)]
    [InlineData(ProjectileKind.Rocket, true, true)]
    [InlineData(ProjectileKind.Bfg, false, true)]
    [InlineData(ProjectileKind.Bfg, true, true)]
    [InlineData(ProjectileKind.Rocket, false, false)]
    [InlineData(ProjectileKind.Rocket, true, false)]
    [InlineData(ProjectileKind.Bfg, false, false)]
    [InlineData(ProjectileKind.Bfg, true, false)]
    public void SkyRemovalSkipsExplosionDamageAndRandomness(ProjectileKind kind, bool ceiling, bool sky)
    {
        var texture = sky ? "f_sky1" : "STONE";
        var sim = AuthoritySimulation.Start(new PlayLevel {
            Sectors = [new LevelSector { CeilingHeight = 128, FloorPic = texture, CeilingPic = texture,
                HealthFloor = 1000, HealthCeiling = 1000 }],
            Things = [new LevelThing { Type = 1 }] });
        var target = sim.AddBot(160, 0); target.Brain = null; target.Health = 10000;
        var missile = sim.SpawnProjectile(sim.Players.Single(), kind);
        missile.X = Fixed.FromInt(100); missile.Z = Fixed.FromInt(ceiling ? 118 : 2);
        missile.VelocityX = missile.VelocityY = default; missile.VelocityZ = Fixed.FromInt(ceiling ? 10 : -10);
        var random = sim.CombatRandomState;
        sim.Tick();
        Assert.True(missile.Destroyed); Assert.Equal(ceiling ? 120 : 0, missile.Z.ToDouble());
        var health = ceiling ? sim.Level.Sectors[0].HealthCeiling : sim.Level.Sectors[0].HealthFloor;
        if (sky)
        {
            Assert.Equal(10000, target.Health); Assert.Equal(1000, health); Assert.Equal(random, sim.CombatRandomState);
        }
        else
        {
            Assert.True(target.Health < 10000); Assert.True(health < 1000); Assert.NotEqual(random, sim.CombatRandomState);
        }
    }
}
