using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ProjectileOwnerBarrierTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissileSpeciesPassageRequiresLiveOwner(bool destroyed)
    {
        var (sim, owner, missile, target) = Setup(); missile.MThruSpecies = true;
        if (destroyed) owner.Destroy();
        Assert.Equal(destroyed ? 2 : (int?)null, ActorJumpActions.CheckBlock(missile, 2));
        missile.DamageExpression = _ => 7;
        missile.Tick();
        Assert.Equal(destroyed, missile.Destroyed); Assert.Equal(destroyed ? 993 : 1000, target.Health);
        Assert.Same(destroyed ? null : owner, missile.Owner);
    }

    [Fact]
    public void DestroyedOwnerIsMissingForActorTargetAndDamageAttribution()
    {
        var (sim, owner, missile, _) = Setup(); owner.Destroy();
        Assert.Null(missile.Owner); Assert.Null(missile.TargetId);
        Assert.Null(AcsActorPointer.Resolve(sim, missile, AcsActorPointer.Target));
        missile.DamageExpression = _ => 7;
        var victim = new DamageObserver { Simulation = sim, Health = 100, NoPain = true };
        missile.DoMissileDamage(victim);
        Assert.Null(victim.Source); Assert.Same(missile, victim.Inflictor); Assert.Equal(93, victim.Health);
    }

    [Fact]
    public void SaveAfterOwnerDestructionPreservesNullOwnership()
    {
        var (sim, owner, missile, _) = Setup(); owner.Destroy();
        var id = missile.Id; var bytes = SimSavegame.Write(sim);
        Assert.Null(missile.Owner);
        SimSavegame.Apply(sim, bytes);
        var restored = sim.Actors.OfType<ProjectileActor>().Single(actor => actor.Id == id);
        Assert.Null(restored.Owner); Assert.Null(restored.TargetId);
        Assert.Equal(bytes, SimSavegame.Write(sim));
    }

    private sealed class DamageObserver : Actor
    {
        public Actor? Source { get; private set; }
        public Actor? Inflictor { get; private set; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        { Source = source; Inflictor = inflictor; return damage; }
    }

    private static (AuthoritySimulation, Actor, ProjectileActor, Actor) Setup()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0, 3001); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(2); missile.VelocityY = missile.VelocityZ = default;
        return (sim, owner, missile, target);
    }
}
