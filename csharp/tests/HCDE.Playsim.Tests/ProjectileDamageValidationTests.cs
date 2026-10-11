using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileDamageValidationTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void MissingDamageFunctionRejectsQueriesAndImpactWithoutRandomOrHealthChanges(int damage)
    {
        var sim = Room(); var target = sim.Players.Single();
        var missile = sim.SpawnProjectile(target, ProjectileKind.Plasma);
        missile.SetDamage(damage);
        Assert.Throws<NotSupportedException>(() => missile.GetMissileDamage(0, 1));
        Assert.Throws<NotSupportedException>(() => missile.GetMissileDamage(7, 1));
        Assert.Throws<NotSupportedException>(() => missile.DoMissileDamage(target));
        Assert.Throws<NotSupportedException>(() => AcsActorProperties.Get(sim, missile, 0, AcsActorProperties.Damage));
        Assert.Throws<NotSupportedException>(() => AcsActorProperties.Check(sim, missile, 0, AcsActorProperties.Damage, 0));
        Assert.Equal(100, target.Health);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NegativeFunctionResultStillHealsAndSetterClearsFunction(bool typed)
    {
        var sim = Room(); var target = sim.Players.Single(); target.Health = 50;
        var missile = sim.SpawnProjectile(target, ProjectileKind.Plasma); missile.Damage = -1;
        var calls = 0;
        if (typed) missile.DamageExpression = _ => { calls++; return -7; };
        else missile.DamageFunction = _ => { calls++; return -7; };
        missile.DoMissileDamage(target);
        Assert.Equal(57, target.Health); Assert.Equal(1, calls);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
        missile.SetDamage(-1);
        Assert.Null(missile.DamageExpression); Assert.Null(missile.DamageFunction);
        Assert.Throws<NotSupportedException>(() => missile.DoMissileDamage(target));
        Assert.Equal(57, target.Health); Assert.Equal(1, calls);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
}
