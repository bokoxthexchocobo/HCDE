using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorDamageTracerTests
{
    [Theory]
    [InlineData(10, true, false, 40)]
    [InlineData(-10, true, false, 60)]
    [InlineData(10, false, false, 50)]
    [InlineData(10, true, true, 50)]
    public void ProjectileTracerActionResolvesLiveTarget(int amount, bool hasTracer, bool destroyed, int expected)
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var owner = sim.Players.Single(); var target = sim.AddBot(100, 0, 3001); target.Health = 50;
        var missile = sim.SpawnProjectile(owner, hasTracer ? ProjectileKind.RevenantTracer : ProjectileKind.Rocket, hasTracer ? target : null);
        if (destroyed) target.Destroy();
        Assert.Same(owner, AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target));
        Assert.Same(hasTracer && !destroyed ? target : null, AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Tracer));
        ActorHealthActions.DamageTracer(missile, amount, sourceSelector: AcsActorPointer.Target);
        Assert.Equal(expected, target.Health); Assert.Equal(100, owner.Health);
    }

    [Fact]
    public void ProjectileDamageTargetUsesOwnerAndDestroyedOwnerIsMissing()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        {
            Things = [new LevelThing { Type = 1 }], Sectors = [new LevelSector { CeilingHeight = 128 }],
        });
        var owner = sim.Players.Single(); var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        ActorHealthActions.DamageTarget(missile, 10); Assert.Equal(90, owner.Health);
        owner.Destroy(); Assert.Null(AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target));
        ActorHealthActions.DamageTarget(missile, 10); Assert.Equal(90, owner.Health);
    }
}
