namespace HCDE.Playsim.Tests;

public class PoisonDeathTypeTests
{
    [Theory]
    [InlineData("Fire", "Ice", false, 3)]
    [InlineData("Fire", "Ice", true, 3)]
    [InlineData("Ice", "Fire", false, 2)]
    [InlineData("Poison", "Fire", false, 1)]
    public void LethalPoisonUsesStoredTypeAndRetainsSourceAttribution(string poisonType, string sourceDeathType, bool specialFire, int state)
    {
        var player = new PlayerPawn { Health = 1, DeathState = 1 };
        player.SetTypedDeath("Fire", 3); player.SetTypedDeath("Ice", 2);
        var source = new Actor { DeathType = sourceDeathType, SpecialFireDamage = specialFire };
        player.ApplyPoison(5, new Actor { DamageType = poisonType }, source);
        var result = ActorDamage.ApplyPoisonDamage(player, 1, source);
        Assert.True(result.Killed); Assert.Equal(1, result.HealthLost);
        Assert.Equal(state == 1 ? null : poisonType, player.DamageType); Assert.Equal(state, player.States.Current);
        Assert.Equal(source.Id, player.LastDamageSourceId);
        Assert.Equal(0, player.DamageCount);
    }

    [Fact]
    public void NullPoisonerDoesNotBorrowSourceDeathType()
    {
        var player = new PlayerPawn { Health = 1, DeathState = 1 };
        player.SetTypedDeath("Fire", 3);
        var source = new Actor { DeathType = "Fire" };
        player.ApplyPoison(5, source: source);
        ActorDamage.ApplyPoisonDamage(player, 1, source);
        Assert.Null(player.DamageType); Assert.Equal(1, player.States.Current);
    }
}
