namespace HCDE.Playsim.Tests;

public class NonplayerArmorDepletionTests
{
    [Fact]
    public void OrdinaryHitClearsAlreadyDepletedArmorProtection()
    {
        var actor = new Actor { Armor = 0, ArmorSavePercent = 50, MaxAbsorb = 20, MaxFullAbsorb = 10 };
        var result = ActorDamage.Apply(actor, 5);
        Assert.Equal(0, result.ArmorLost);
        Assert.Equal(5, result.HealthLost);
        Assert.Equal(0, actor.ArmorSavePercent);
        Assert.Equal(20, actor.MaxAbsorb);
        Assert.Equal(10, actor.MaxFullAbsorb);
    }

    [Theory]
    [InlineData(DamageFlags.BypassArmor)]
    [InlineData(DamageFlags.Forced)]
    public void ArmorBypassDoesNotRunDepletionCleanup(DamageFlags flags)
    {
        var actor = new Actor { Armor = 0, ArmorSavePercent = 50 };
        ActorDamage.Apply(actor, 5, flags: flags);
        Assert.Equal(50, actor.ArmorSavePercent);
    }
}
