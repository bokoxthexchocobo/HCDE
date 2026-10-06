using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DeathSkullChargeTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LethalDamageClearsChargeBeforeNextTick(bool noAutoOff)
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        soul.NoAutoOffSkullFly = noAutoOff;
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        ActorDamage.Apply(soul, 1000);
        Assert.True(soul.IsDead);
        Assert.False(soul.Brain.Charging);
        Assert.Equal(default, soul.VelocityX);
        Assert.Equal(default, soul.VelocityY);
        Assert.Equal(default, soul.VelocityZ);
    }

    [Fact]
    public void DeathFlagClearDoesNotDiscardVelocitySetByDamageHook()
    {
        var actor = new DeathObserver { Brain = MonsterBrain.ForType(3006), Health = 100 };
        actor.Brain!.StartCharge(actor, new Actor { X = Fixed.FromInt(800) });
        ActorDamage.Apply(actor, 1000);
        Assert.False(actor.Brain.Charging);
        Assert.False(actor.ChargingAtDeathQuery);
        Assert.Equal(Fixed.FromInt(3), actor.VelocityX);
        Assert.Equal(Fixed.FromInt(4), actor.VelocityY);
        Assert.Equal(Fixed.FromInt(5), actor.VelocityZ);
    }

    [Fact]
    public void NonlethalNoPainDamageResetsVelocityWithoutClearingCharge()
    {
        var sim = Room(); var soul = sim.Actors.Single(a => a.DoomEdNum == 3006);
        soul.Brain!.StartCharge(soul, sim.Players.Single());
        ActorDamage.Apply(soul, 1, flags: DamageFlags.NoPain);
        Assert.False(soul.IsDead);
        Assert.True(soul.Brain.Charging);
        Assert.Equal(default, soul.VelocityX);
        Assert.Equal(default, soul.VelocityY);
        Assert.Equal(default, soul.VelocityZ);
    }

    private sealed class DeathObserver : Actor
    {
        public bool ChargingAtDeathQuery { get; private set; } = true;

        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage,
            string? damageType, DamageFlags flags)
        {
            VelocityX = Fixed.FromInt(3);
            VelocityY = Fixed.FromInt(4);
            VelocityZ = Fixed.FromInt(5);
            return damage;
        }

        public override int GetGibHealth()
        {
            ChargingAtDeathQuery = Brain!.Charging;
            return base.GetGibHealth();
        }
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 512 }],
        Things = [new LevelThing { Type = 1, X = 800 }, new LevelThing { Type = 3006, Z = 100 }],
    });
}
