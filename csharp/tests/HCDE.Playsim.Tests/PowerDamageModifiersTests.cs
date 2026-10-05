namespace HCDE.Playsim.Tests;

public class PowerDamageModifiersTests
{
    [Theory]
    [InlineData(false, false, 28)]
    [InlineData(false, true, 7)]
    [InlineData(true, false, 7)]
    [InlineData(true, true, 1)]
    public void DefaultsRespectActivePassiveStage(bool protection, bool passive, int expected)
    {
        InventoryDamageModifier power = protection ? new PowerProtection() : new PowerDamage();
        Assert.Equal(expected, power.ModifyDamage(7, "Fire", passive, null, null, DamageFlags.None));
    }

    [Fact]
    public void PopulatedTableUsesExactThenNoneThenIdentity()
    {
        var power = new PowerDamage();
        power.SetDamageFactor("Fire", 0.5);
        Assert.Equal(10, power.ModifyDamage(20, "fIrE", false, null, null, DamageFlags.None));
        Assert.Equal(20, power.ModifyDamage(20, "Ice", false, null, null, DamageFlags.None));
        power.SetDamageFactor("None", 2);
        Assert.Equal(40, power.ModifyDamage(20, "Ice", false, null, null, DamageFlags.None));
        Assert.Equal(10, power.ModifyDamage(20, "Fire", false, null, null, DamageFlags.None));
        Assert.Equal(40, power.ModifyDamage(20, null, false, null, null, DamageFlags.None));
    }

    [Fact]
    public void FloorsDifferAndNegativeFactorsMeanIdentity()
    {
        var damage = new PowerDamage();
        var protection = new PowerProtection();
        damage.SetDamageFactor("None", 0);
        protection.SetDamageFactor("None", 0);
        Assert.Equal(1, damage.ModifyDamage(20, null, false, null, null, DamageFlags.None));
        Assert.Equal(0, protection.ModifyDamage(20, null, true, null, null, DamageFlags.None));
        damage.SetDamageFactor("None", -1);
        Assert.Equal(20, damage.ModifyDamage(20, null, false, null, null, DamageFlags.None));
    }

    [Theory]
    [InlineData(DamageFlags.None, 20)]
    [InlineData(DamageFlags.NoEnhance, 5)]
    [InlineData(DamageFlags.NoProtect, 80)]
    public void PowersRunThroughSharedDamageAndBypassFlags(DamageFlags flags, int expected)
    {
        var target = new Actor { Health = 100, DamageModifiers = new PowerProtection() };
        var source = new Actor { DamageModifiers = new PowerDamage() };
        Assert.Equal(expected, ActorDamage.Apply(target, 20, source, flags).HealthLost);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void NonpositiveDamageIsUnchanged(int value)
    {
        Assert.Equal(value, new PowerDamage().ModifyDamage(value, null, false, null, null, DamageFlags.None));
        Assert.Equal(value, new PowerProtection().ModifyDamage(value, null, true, null, null, DamageFlags.None));
    }
}
