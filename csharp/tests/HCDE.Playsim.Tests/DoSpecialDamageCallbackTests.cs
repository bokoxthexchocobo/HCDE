namespace HCDE.Playsim.Tests;

public class DoSpecialDamageCallbackTests
{
    private sealed class Inflictor : Actor
    {
        public int Result { get; init; }
        public int Calls { get; private set; }
        public override int DoSpecialDamage(Actor target, int damage, string? damageType, DamageFlags flags)
        { Calls++; Assert.Equal("Fire", damageType); Assert.Equal(20, damage); return Result; }
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, 0)]
    [InlineData(10, 10)]
    public void InflictorAdjustsDamageBeforeSourceAndTargetFactors(int replacement, int expected)
    {
        var target = new Actor { Health = 100, DamageFactor = Fixed.FromDouble(0.5) };
        var source = new Actor { DamageMultiplier = Fixed.FromInt(2) };
        var inflictor = new Inflictor { Result = replacement };
        Assert.Equal(expected, ActorDamage.Apply(target, 20, source, damageType: "Fire", inflictor: inflictor).HealthLost);
        Assert.Equal(1, inflictor.Calls);
    }

    [Fact]
    public void ForcedDamageSkipsInflictorCallback()
    {
        var inflictor = new Inflictor { Result = -1 };
        Assert.Equal(20, ActorDamage.Apply(new Actor { Health = 100 }, 20, flags: DamageFlags.Forced,
            damageType: "Fire", inflictor: inflictor).HealthLost);
        Assert.Equal(0, inflictor.Calls);
    }
}
