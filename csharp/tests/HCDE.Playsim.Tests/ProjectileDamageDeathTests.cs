using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileDamageDeathTests
{
    [Theory]
    [InlineData(ProjectileKind.Plasma)]
    [InlineData(ProjectileKind.Rocket)]
    [InlineData(ProjectileKind.Bfg)]
    public void DamageDeathUsesExplosionWithoutActorDeathBookkeeping(ProjectileKind kind)
    {
        var sim = Room(); var owner = sim.Players.Single(); var missile = sim.SpawnProjectile(owner, kind);
        missile.Shootable = true; missile.IsMonster = true; missile.Special = 130;
        var height = missile.Height; var killer = sim.AddBot(600, 0);
        Assert.True(ActorDamage.Apply(missile, 1000, killer).Killed);
        Assert.True(missile.Destroyed); Assert.Same(owner, missile.Owner);
        Assert.Equal(height, missile.Height); Assert.False(missile.Corpse); Assert.False(missile.Killed);
        Assert.Equal(0, missile.DeathCount); Assert.Equal(130, missile.Special);
        Assert.Equal(0, missile.VelocityX.Raw); Assert.Equal(0, missile.VelocityY.Raw); Assert.Equal(0, missile.VelocityZ.Raw);
    }

    [Fact]
    public void RocketDamageDeathAppliesBlastWithOriginalOwnerCredit()
    {
        var sim = Room(); var owner = sim.Players.Single(); var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        missile.X = Fixed.FromInt(200); missile.Y = default; missile.Z = default; missile.Shootable = true;
        var victim = sim.AddBot(220, 0); victim.Brain = null; var killer = sim.AddBot(600, 0);
        ActorDamage.Apply(missile, 1000, killer);
        Assert.True(victim.Health < victim.ResurrectionHealth); Assert.Equal(owner.Id, victim.LastDamageSourceId);
    }

    [Fact]
    public void SurvivingMissileDamageDoesNotExplode()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.Shootable = true; ActorDamage.Apply(missile, 1);
        Assert.False(missile.Destroyed); Assert.True(missile.Health > 0);
    }

    [Fact]
    public void ExplosionCountGateRunsBeforeMotionCleanupOnDamageDeath()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.Shootable = true; missile.ExplodeCount = true; missile.SetChaseThreshold(3, setDefault: true);
        missile.SetChaseThreshold(0, setDefault: false); missile.VelocityX = Fixed.FromInt(7);
        ActorDamage.Apply(missile, 1000);
        Assert.Equal(1, missile.ChaseThreshold); Assert.False(missile.Destroyed);
        Assert.Equal(Fixed.FromInt(7), missile.VelocityX); Assert.True(missile.Shootable);
        Assert.False(missile.Killed); Assert.Equal(0, missile.DeathCount);
    }

    [Fact]
    public void MissileDeathDoesNotConsumeActorDieRandomStream()
    {
        var sim = Room(); var missile = sim.SpawnProjectile(sim.Players.Single(), ProjectileKind.Plasma);
        missile.Shootable = true; ActorDamage.Apply(missile, 1000);
        Assert.Null(sim.CaptureState().ActorDieRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 256 }], Things = [new LevelThing { Type = 1, X = -200 }]
    });
}
