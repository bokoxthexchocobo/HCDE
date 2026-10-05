namespace HCDE.Playsim.Tests;

public class RetainedDeathDamageTypeTests
{
    [Fact]
    public void SuppressedSpecialFireRetainsPreviousActorType()
    {
        var player = new PlayerPawn { Health = 10, DamageType = "Ice" };
        player.SetTypedDeath("Ice", 3);
        ActorDamage.Apply(player, 10, damageType: "Fire", inflictor: new Actor { SpecialFireDamage = true });
        Assert.Equal(3, player.States.Current);
        Assert.Equal("Ice", player.DamageType);
    }

    [Fact]
    public void AcceptedSpecialFireReplacesPreviousActorType()
    {
        var player = new PlayerPawn { Health = 26, DamageType = "Ice" };
        player.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(player, 26, damageType: "Fire", inflictor: new Actor { SpecialFireDamage = true });
        Assert.Equal(3, player.States.Current);
        Assert.Equal("Fire", player.DamageType);
    }

    [Theory]
    [InlineData("Fire", null)]
    [InlineData("Extreme", null)]
    [InlineData("Massacre", "Massacre")]
    public void FallbackClearsUnsupportedTypesButPreservesMassacre(string type, string? retained)
    {
        var actor = new Actor { Health = 10 };
        ActorDamage.Apply(actor, 10, damageType: type);
        Assert.Equal(retained, actor.DamageType);
    }

    [Fact]
    public void SurvivingHitDoesNotOverwritePersistentType()
    {
        var actor = new Actor { Health = 100, DamageType = "Ice" };
        ActorDamage.Apply(actor, 10, damageType: "Fire");
        Assert.Equal("Ice", actor.DamageType);
    }
}
