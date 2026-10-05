namespace HCDE.Playsim.Tests;

public class AcsSpecialDamageFlagTests
{
    [Theory]
    [InlineData("foilinvul", "FOILINVUL")]
    [InlineData("specialfiredamage", "SPECIALFIREDAMAGE")]
    public void FlagsRoundTripThroughNativeCaseInsensitiveNames(string write, string read)
    {
        var actor = new Actor();
        Assert.True(AcsActorFlags.TrySet(actor, write, true));
        Assert.True(AcsActorFlags.TryGet(actor, read, out var enabled));
        Assert.True(enabled);
        Assert.True(AcsActorFlags.TrySet(actor, read, false));
        Assert.True(AcsActorFlags.TryGet(actor, write, out enabled));
        Assert.False(enabled);
    }

    [Fact]
    public void ScriptFoilInvulChangesDamageBehavior()
    {
        var inflictor = new Actor();
        var target = new Actor { Health = 100, Invulnerable = true };
        Assert.True(AcsActorFlags.TrySet(inflictor, "FOILINVUL", true));
        Assert.Equal(20, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
        Assert.True(AcsActorFlags.TrySet(inflictor, "FOILINVUL", false));
        Assert.Equal(0, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
    }

    [Fact]
    public void ScriptSpecialFireDamageChangesPlayerDeathSelection()
    {
        var inflictor = new Actor();
        Assert.True(AcsActorFlags.TrySet(inflictor, "SPECIALFIREDAMAGE", true));
        var target = new PlayerPawn { Health = 10 };
        target.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(target, 10, damageType: "Fire", inflictor: inflictor);
        Assert.Equal(2, target.States.Current);
    }
}
