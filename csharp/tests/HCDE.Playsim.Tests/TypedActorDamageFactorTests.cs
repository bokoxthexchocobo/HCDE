namespace HCDE.Playsim.Tests;

public class TypedActorDamageFactorTests
{
    [Theory]
    [InlineData("fIrE", 5)]
    [InlineData("Ice", 20)]
    [InlineData(null, 20)]
    [InlineData("None", 20)]
    public void ExactFactorsOverrideNoneFallback(string? type, int expected)
    {
        var target = new Actor { Health = 100, DamageFactor = Fixed.FromDouble(0.5) };
        target.SetDamageFactor("Fire", 0.25); target.SetDamageFactor("None", 1);
        Assert.Equal(expected, ActorDamage.Apply(target, 40, damageType: type).HealthLost);
    }

    [Fact]
    public void ScalarAndTypedFactorsTruncateSeparatelyBeforeArmor()
    {
        var target = new Actor { Health = 100, DamageFactor = Fixed.FromDouble(0.5), Armor = 100, ArmorSavePercent = 50 };
        target.SetDamageFactor("Fire", 1.5);
        var result = ActorDamage.Apply(target, 7, damageType: "Fire");
        Assert.Equal(2, result.HealthLost); Assert.Equal(2, result.ArmorLost);
    }

    [Theory]
    [InlineData(DamageFlags.NoFactor, 20)]
    [InlineData(DamageFlags.Forced, 20)]
    [InlineData(DamageFlags.None, 0)]
    public void BypassFlagsSkipBothFactors(DamageFlags flags, int expected)
    {
        var target = new Actor { Health = 100 };
        target.SetDamageFactor("Fire", 0);
        Assert.Equal(expected, ActorDamage.Apply(target, 20, flags: flags, damageType: "Fire").HealthLost);
    }

    [Fact]
    public void NegativeNoneFallbackIsIgnoredButExactNegativeCancelsDamage()
    {
        var target = new Actor { Health = 100 };
        target.SetDamageFactor("None", -1);
        Assert.Equal(20, ActorDamage.Apply(target, 20, damageType: "Ice").HealthLost);
        target.SetDamageFactor("Fire", -1);
        Assert.Equal(0, ActorDamage.Apply(target, 20, damageType: "Fire").HealthLost);
    }

    [Fact]
    public void UnknownTypeWithoutFallbackUsesIdentity()
    {
        var target = new Actor { Health = 100 }; target.SetDamageFactor("Fire", 0);
        Assert.Equal(20, ActorDamage.Apply(target, 20, damageType: "Ice").HealthLost);
    }
}
