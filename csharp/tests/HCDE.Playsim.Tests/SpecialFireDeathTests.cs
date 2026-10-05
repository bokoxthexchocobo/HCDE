namespace HCDE.Playsim.Tests;

public class SpecialFireDeathTests
{
    [Theory]
    [InlineData(25, 25, true, 2)]
    [InlineData(26, 26, true, 3)]
    [InlineData(1, 50, true, 3)]
    [InlineData(1, 51, true, 2)]
    [InlineData(1, 51, false, 3)]
    public void PlayerSpecialFireUsesNativeDamageAndOverkillBoundaries(int health, int damage, bool special, int state)
    {
        var player = new PlayerPawn { Health = health };
        player.SetTypedDeath("Fire", 3);
        Assert.True(ActorDamage.Apply(player, damage, damageType: "Fire",
            inflictor: new Actor { SpecialFireDamage = special }).Killed);
        Assert.Equal(state, player.States.Current);
    }

    [Fact]
    public void SpecialFireThresholdUsesPostArmorDamage()
    {
        var player = new PlayerPawn { Health = 10 };
        player.Inventory.Armor = 100;
        player.Inventory.ArmorSavePercent = 50;
        player.SetTypedDeath("Fire", 3);
        var result = ActorDamage.Apply(player, 30, damageType: "Fire", inflictor: new Actor { SpecialFireDamage = true });
        Assert.True(result.Killed);
        Assert.Equal(15, result.ArmorLost);
        Assert.Equal(2, player.States.Current);
    }

    [Fact]
    public void FireWithoutInflictorKeepsTypedDeathAtLowDamage()
    {
        var player = new PlayerPawn { Health = 1 };
        player.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(player, 1, damageType: "Fire");
        Assert.Equal(3, player.States.Current);
    }

    [Fact]
    public void MonsterSpecialFireDoesNotUsePlayerRestriction()
    {
        var actor = new Actor { Health = 1, IsMonster = true };
        actor.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(actor, 10, damageType: "Fire", inflictor: new Actor { SpecialFireDamage = true });
        Assert.Equal(3, actor.States.Current);
    }

    [Fact]
    public void OverrideFireAlsoUsesSpecialFireRestriction()
    {
        var player = new PlayerPawn { Health = 10 };
        player.SetTypedDeath("Fire", 3);
        ActorDamage.Apply(player, 10, damageType: "Ice", inflictor: new Actor { SpecialFireDamage = true, DeathType = "fIrE" });
        Assert.Equal(2, player.States.Current);
    }
}
