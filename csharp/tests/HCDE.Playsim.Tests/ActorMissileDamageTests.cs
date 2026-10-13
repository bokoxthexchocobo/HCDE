using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class ActorMissileDamageTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void OrdinaryActorUsesMissileDiceAndTargetAsSource(bool rip, bool strife)
    {
        var sim = Room(); var actor = sim.AddBot(100, 0, 3001); actor.Brain = null;
        var source = sim.Players.Single(); actor.SetTargetPointer(source);
        actor.Damage = 5; actor.Rip = rip; actor.StrifeDamage = strife;
        actor.ActorSounds = new Dictionary<ActorSoundType, string> { [ActorSoundType.Rip] = "custom/rip" };
        var victim = new DamageObserver { Simulation = sim, Health = 1000, NoPain = true };
        var prediction = Room(); var random = prediction.NextMissileDamageByte();
        var expected = 5 * ((random & (rip || strife ? 3 : 7)) + (rip ? 2 : 1));
        sim.DrainSoundRequests();
        Assert.Equal(expected, actor.DoMissileDamage(victim).HealthLost);
        Assert.Same(source, victim.Source); Assert.Same(actor, victim.Inflictor);
        Assert.Equal(prediction.CaptureState().MissileDamageRandomState, sim.CaptureState().MissileDamageRandomState);
        var sounds = sim.DrainSoundRequests();
        if (rip) Assert.Equal("custom/rip", Assert.Single(sounds).Sound);
        else Assert.Empty(sounds);
    }

    [Theory]
    [InlineData(-7)]
    [InlineData(0)]
    [InlineData(7)]
    public void OrdinaryActorDamageFunctionSupportsHealingAndBypassesRandom(int damage)
    {
        var sim = Room(); var actor = sim.AddBot(100, 0, 3001); actor.Brain = null;
        actor.SetTargetPointer(sim.Players.Single()); actor.Rip = true;
        var calls = 0; actor.DamageFunction = self => { Assert.Same(actor, self); calls++; return damage; };
        var victim = sim.Players.Single(); victim.Health = 50; victim.NoPain = true;
        actor.DoMissileDamage(victim);
        Assert.Equal(50 - damage, victim.Health); Assert.Equal(1, calls);
        Assert.Null(sim.CaptureState().MissileDamageRandomState);
    }

    [Fact]
    public void TargetChangedByDamageFunctionControlsAttribution()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0, 3001); actor.Brain = null;
        var source = sim.Players.Single();
        actor.DamageFunction = _ => { actor.SetTargetPointer(source); return 7; };
        var victim = new DamageObserver { Simulation = sim, Health = 1000, NoPain = true };
        actor.DoMissileDamage(victim);
        Assert.Same(source, victim.Source); Assert.Same(actor, victim.Inflictor);
    }

    [Fact]
    public void MissingTargetKeepsNullSourceAndActorInflictor()
    {
        var sim = Room(); var actor = sim.AddBot(100, 0, 3001); actor.Brain = null;
        actor.DamageFunction = _ => 7;
        var victim = new DamageObserver { Simulation = sim, Health = 1000, NoPain = true };
        actor.DoMissileDamage(victim);
        Assert.Null(victim.Source); Assert.Same(actor, victim.Inflictor);
    }

    private sealed class DamageObserver : Actor
    {
        public Actor? Source { get; private set; }
        public Actor? Inflictor { get; private set; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        {
            Source = source; Inflictor = inflictor; return damage;
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] }, rngSeed: 7);
}
