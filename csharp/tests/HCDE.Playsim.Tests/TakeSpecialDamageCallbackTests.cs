namespace HCDE.Playsim.Tests;

public class TakeSpecialDamageCallbackTests
{
    private sealed class CallbackActor : Actor
    {
        public int Result { get; init; }
        public int Calls { get; private set; }
        public int ReceivedDamage { get; private set; }
        public Actor? ReceivedSource { get; private set; }
        public Actor? ReceivedInflictor { get; private set; }
        public string? ReceivedType { get; private set; }
        public DamageFlags ReceivedFlags { get; private set; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        {
            Calls++; ReceivedDamage = damage; ReceivedSource = source; ReceivedInflictor = inflictor;
            ReceivedType = damageType; ReceivedFlags = flags; return Result;
        }
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(12, 12)]
    public void CallbackCanRejectOrReplaceDamage(int result, int expected)
    {
        var actor = new CallbackActor { Health = 100, Result = result };
        Assert.Equal(expected, ActorDamage.Apply(actor, 20).HealthLost);
        Assert.Equal(1, actor.Calls); Assert.Equal(100 - expected, actor.Health);
    }

    [Fact]
    public void CallbackReceivesScaledDamageAndOriginalContextBeforeArmor()
    {
        var actor = new CallbackActor { Health = 100, Result = 12, DamageFactor = Fixed.FromDouble(0.5),
            Armor = 100, ArmorSavePercent = 50 };
        var source = new Actor { DamageMultiplier = Fixed.FromInt(2) }; var inflictor = new Actor();
        var result = ActorDamage.Apply(actor, 20, source, DamageFlags.NoPain, "Fire", inflictor);
        Assert.Equal(20, actor.ReceivedDamage); Assert.Same(source, actor.ReceivedSource);
        Assert.Same(inflictor, actor.ReceivedInflictor); Assert.Equal("Fire", actor.ReceivedType);
        Assert.Equal(DamageFlags.NoPain, actor.ReceivedFlags);
        Assert.Equal(6, result.HealthLost); Assert.Equal(6, result.ArmorLost);
    }

    [Fact]
    public void ForcedDamageBypassesCallback()
    {
        var actor = new CallbackActor { Health = 100, Result = -1 };
        Assert.Equal(20, ActorDamage.Apply(actor, 20, flags: DamageFlags.Forced).HealthLost);
        Assert.Equal(0, actor.Calls);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(10, 10)]
    public void ZeroAfterTargetFactorStillReachesCallback(int replacement, int expected)
    {
        var actor = new CallbackActor { Health = 100, Result = replacement, DamageFactor = Fixed.FromInt(0) };
        Assert.Equal(expected, ActorDamage.Apply(actor, 20).HealthLost);
        Assert.Equal(1, actor.Calls); Assert.Equal(0, actor.ReceivedDamage);
    }

    [Fact]
    public void ZeroAfterSourceFactorStillReachesCallback()
    {
        var actor = new CallbackActor { Health = 100, Result = 10 };
        Assert.Equal(10, ActorDamage.Apply(actor, 20, new Actor { DamageMultiplier = Fixed.FromInt(0) }).HealthLost);
        Assert.Equal(1, actor.Calls); Assert.Equal(0, actor.ReceivedDamage);
    }

    [Fact]
    public void InitiallyZeroHitReachesCallbackAndCanSupplyDamage()
    {
        var actor = new CallbackActor { Health = 100, Result = 10 };
        Assert.Equal(10, ActorDamage.Apply(actor, 0).HealthLost); Assert.Equal(1, actor.Calls);
        Assert.Equal(0, actor.ReceivedDamage);
    }

    [Fact]
    public void DefaultInitiallyZeroHitLeavesHealthAndArmorUnchanged()
    {
        var actor = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(default, ActorDamage.Apply(actor, 0));
        Assert.Equal(100, actor.Health); Assert.Equal(100, actor.Armor);
    }

    [Fact]
    public void InvulnerabilityStillBlocksInitiallyZeroCallback()
    {
        var actor = new CallbackActor { Health = 100, Result = 10, Invulnerable = true };
        Assert.Equal(0, ActorDamage.Apply(actor, 0).HealthLost); Assert.Equal(0, actor.Calls);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void NegativeInputNormalizesToZeroBeforeCallback(int input)
    {
        var actor = new CallbackActor { Health = 100, Result = 10 };
        Assert.Equal(10, ActorDamage.Apply(actor, input).HealthLost);
        Assert.Equal(1, actor.Calls); Assert.Equal(0, actor.ReceivedDamage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void DefaultNegativeInputDoesNotHealOrSpendArmor(int input)
    {
        var actor = new Actor { Health = 80, Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(default, ActorDamage.Apply(actor, input));
        Assert.Equal(80, actor.Health); Assert.Equal(100, actor.Armor);
    }

    [Fact]
    public void NegativeInputStillHonorsInvulnerabilityBeforeCallback()
    {
        var actor = new CallbackActor { Health = 100, Result = 10, Invulnerable = true };
        Assert.Equal(0, ActorDamage.Apply(actor, -1).HealthLost); Assert.Equal(0, actor.Calls);
    }
}
