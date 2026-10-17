using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ReflectorPassageTests
{
    [Theory]
    [InlineData(false, 993)]
    [InlineData(true, 1000)]
    public void ReflectorThruFlagTransfersOwnerWithoutChangingVelocityOrExploding(bool invulnerable, int health)
    {
        var (_, missile, target) = Room(); var owner = missile.Owner; target.Invulnerable = invulnerable;
        missile.Tick();
        Assert.False(missile.Destroyed); Assert.Same(target, missile.Owner); Assert.Equal(health, target.Health);
        Assert.Equal(0, missile.X.Raw); Assert.Equal(8, missile.VelocityX.ToDouble());
        if (!invulnerable) Assert.Equal(owner!.Id, target.LastDamageSourceId);
    }

    [Fact]
    public void NonshootableReflectorStillTransfersOwnerAfterBlocking()
    {
        var (_, missile, target) = Room(); target.Shootable = false;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Same(target, missile.Owner); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void ExplicitMissileStopCallbackStillReachesReflectorHandling()
    {
        var (_, missile, target) = Room(); missile.SpecialMissileHitFunction = (_, _) => 0;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Same(target, missile.Owner); Assert.Equal(1000, target.Health);
    }

    [Fact]
    public void MissileThruFlagTakesEarlierPassThroughBranchInsteadOfEndingTic()
    {
        var (_, missile, target) = Room(); missile.ThruReflect = true;
        missile.Tick(); Assert.False(missile.Destroyed); Assert.Same(target, missile.Owner); Assert.Equal(8, missile.X.ToDouble());
    }

    [Fact]
    public void NewOwnerImmunityAllowsTravelOnFollowingTic()
    {
        var (_, missile, target) = Room(); missile.Tick(); missile.Tick();
        Assert.Equal(8, missile.X.ToDouble()); Assert.Equal(993, target.Health); Assert.False(missile.Destroyed);
    }

    [Fact]
    public void ReflectionActionsPreserveIndependentInvulnerabilityAndSaveExplicitClear()
    {
        var (sim, _, target) = Room();
        ActorPropertyActions.SetReflectiveInvulnerable(target); ActorPropertyActions.UnsetReflective(target);
        Assert.True(target.Invulnerable); Assert.False(target.Reflective);
        ActorPropertyActions.SetReflective(target); ActorPropertyActions.UnsetReflectiveInvulnerable(target);
        Assert.False(target.Invulnerable); Assert.False(target.Reflective);
        var saved = SimSavegame.Write(sim, true);
        ActorPropertyActions.SetReflectiveInvulnerable(target); SimSavegame.Apply(sim, saved);
        Assert.False(target.Invulnerable); Assert.False(target.Reflective); Assert.Equal(saved, SimSavegame.Write(sim, true));
    }

    [Fact]
    public void ReflectionActionCanRunFromStateFrame()
    {
        var (_, _, target) = Room(); target.Reflective = false;
        target.States.Configure(target, [new(-1, 0), new(-1, 0, Action: ActorPropertyActions.SetReflectiveInvulnerable)], 0);
        target.States.Enter(target, 1); Assert.True(target.Reflective); Assert.True(target.Invulnerable);
    }

    private static (AuthoritySimulation Sim, ProjectileActor Missile, Actor Target) Room()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel { Sectors = [new LevelSector { CeilingHeight = 128 }] });
        var owner = sim.AddBot(-200, 0); owner.Brain = null;
        var target = sim.AddBot(10, 0, 3001); target.Brain = null; target.Health = 1000; target.NoPain = true;
        target.Reflective = target.ThruReflect = true;
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Plasma);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        missile.VelocityX = Fixed.FromInt(8); missile.VelocityY = missile.VelocityZ = default; missile.DamageExpression = _ => 7;
        sim.Thinkers.Clock.Restore(1);
        return (sim, missile, target);
    }
}
