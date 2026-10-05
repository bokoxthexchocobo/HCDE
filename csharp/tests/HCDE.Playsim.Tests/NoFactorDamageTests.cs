namespace HCDE.Playsim.Tests;

public class NoFactorDamageTests
{
    [Theory]
    [InlineData(false, 10)]
    [InlineData(true, 40)]
    public void NoFactorSkipsTargetScalingButPreservesSourceScaling(bool noFactor, int expected)
    {
        var actor = new Actor { Health = 100, DamageFactor = Fixed.FromDouble(0.25) };
        var source = new Actor { DamageMultiplier = Fixed.FromInt(2) };
        Assert.Equal(expected, ActorDamage.Apply(actor, 20, source,
            noFactor ? DamageFlags.NoFactor : DamageFlags.None).HealthLost);
    }

    [Fact]
    public void NoFactorStillUsesArmor()
    {
        var actor = new Actor { Health = 100, DamageFactor = Fixed.FromInt(0), Armor = 100, ArmorSavePercent = 50 };
        var result = ActorDamage.Apply(actor, 20, flags: DamageFlags.NoFactor);
        Assert.Equal(10, result.HealthLost); Assert.Equal(10, result.ArmorLost);
    }

    private sealed class CallbackActor : Actor
    {
        public int Input { get; private set; }
        public DamageFlags Flags { get; private set; }
        public override int TakeSpecialDamage(Actor? inflictor, Actor? source, int damage, string? damageType, DamageFlags flags)
        { Input = damage; Flags = flags; return damage / 2; }
    }

    [Fact]
    public void NoFactorStillDispatchesTargetCallbackWithFlags()
    {
        var actor = new CallbackActor { Health = 100, DamageFactor = Fixed.FromInt(0) };
        Assert.Equal(10, ActorDamage.Apply(actor, 20, flags: DamageFlags.NoFactor).HealthLost);
        Assert.Equal(20, actor.Input); Assert.Equal(DamageFlags.NoFactor, actor.Flags);
    }
}
