using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorCountdownArgTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(-1, -2)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void NonzeroArgumentsDecrementWithoutDeath(int value, int expected)
    {
        var actor = new Actor(); actor.SpecialArgs[0] = value;
        ActorPropertyActions.CountdownArg(actor, 0);
        Assert.Equal(expected, actor.SpecialArgs[0]); Assert.False(actor.Killed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public void InvalidArgumentHasNativeNoopBehavior(int index)
    {
        var actor = new Actor(); ActorPropertyActions.CountdownArg(actor, index);
        Assert.All(actor.SpecialArgs, value => Assert.Equal(0, value)); Assert.False(actor.Killed);
        Assert.False(actor.SpecialChanged);
    }

    [Fact]
    public void ZeroBeforeDecrementKillsThroughProtection()
    {
        var actor = new Actor { Invulnerable = true, Buddha = true };
        ActorPropertyActions.CountdownArg(actor, 2, 0);
        Assert.True(actor.Killed); Assert.Equal(-1, actor.SpecialArgs[2]); Assert.Equal(0, actor.Health);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NonshootableActorUsesDefaultOrSelectedState(bool explicitState)
    {
        var actor = new Actor { Shootable = false };
        ActorPropertyActions.CountdownArg(actor, 0, explicitState ? 1 : null);
        Assert.Equal(explicitState ? 1 : actor.DeathState, actor.States.Current);
        Assert.False(actor.Killed); Assert.Equal(100, actor.Health);
    }

    [Fact]
    public void FrameCountdownAndSavedZeroTriggerOnNextInvocation()
    {
        var sim = Room(); var actor = sim.AddBot(0, 0);
        ActorPropertyActions.SetArg(actor, 0, 1);
        actor.States.Configure(actor, [new(-1, 0), new(5, 0, Action: self => ActorPropertyActions.CountdownArg(self, 0)), new(-1, 2)], 0);
        actor.States.Enter(actor, 1); Assert.False(actor.Killed);
        var bytes = SimSavegame.Write(sim);
        Assert.True(SimSavegame.TryRead(bytes, out var state, out var error), error);
        actor.SpecialArgs[0] = 50; sim.RestoreState(state);
        Assert.Equal(0, actor.SpecialArgs[0]); Assert.Equal(bytes, SimSavegame.Write(sim));
        ActorPropertyActions.CountdownArg(actor, 0); Assert.True(actor.Killed);
    }

    [Fact]
    public void ProjectileCountdownUsesExplosionRatherThanDamageEligibility()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1, X = -200 }] });
        var owner = sim.Players.Single(); var target = sim.AddBot(20, 0, 3004);
        var missile = sim.SpawnProjectile(owner, ProjectileKind.Rocket);
        missile.X = missile.Y = default; missile.Z = Fixed.FromInt(20);
        var health = target.Health; ActorPropertyActions.CountdownArg(missile, 0);
        Assert.True(missile.Destroyed);
        Assert.True(target.Health < health, $"Health {health}->{target.Health}, eligible {target.CanTakeDamage}, blockmap {target.IsBlockmapActor}, immune {target.SplashImmune(missile)}");
        Assert.Equal(-1, missile.SpecialArgs[0]);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }] });
}
