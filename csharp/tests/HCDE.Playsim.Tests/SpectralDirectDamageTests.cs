namespace HCDE.Playsim.Tests;

public class SpectralDirectDamageTests
{
    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(true, false, 0)]
    [InlineData(false, true, 20)]
    [InlineData(true, true, 20)]
    public void SpectralTargetRequiresSpectralInflictorRatherThanSource(bool sourceSpectral, bool inflictorSpectral, int expected)
    {
        var target = new Actor { Health = 100, Spectral = true };
        Assert.Equal(expected, ActorDamage.Apply(target, 20,
            new Actor { Spectral = sourceSpectral }, inflictor: new Actor { Spectral = inflictorSpectral }).HealthLost);
    }

    [Fact]
    public void EnvironmentalDamageCannotHurtSpectralTarget()
    {
        var target = new Actor { Health = 100, Spectral = true, Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(default, ActorDamage.Apply(target, 20));
        Assert.Equal(100, target.Health); Assert.Equal(100, target.Armor);
    }

    [Fact]
    public void ForcedDamageBypassesSpectralProtection()
    {
        var target = new Actor { Health = 100, Spectral = true };
        Assert.Equal(20, ActorDamage.Apply(target, 20, flags: DamageFlags.Forced).HealthLost);
    }

    [Fact]
    public void TelefragBypassesSpectralProtection()
    {
        var target = new Actor { Health = 100, Spectral = true };
        Assert.True(ActorDamage.Apply(target, ActorDamage.TelefragDamage).Killed);
    }
}
