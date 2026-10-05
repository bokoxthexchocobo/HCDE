namespace HCDE.Playsim.Tests;

public class PierceArmorTests
{
    [Theory]
    [InlineData(false, false, 10, 90)]
    [InlineData(false, true, 20, 100)]
    [InlineData(true, false, 10, 90)]
    [InlineData(true, true, 20, 100)]
    public void InflictorPiercesPlayerAndMonsterArmor(bool playerTarget, bool pierce, int lost, int armor)
    {
        Actor target = playerTarget ? new PlayerPawn() : new Actor();
        target.Health = 100;
        if (target is PlayerPawn player) { player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50; }
        else { target.Armor = 100; target.ArmorSavePercent = 50; }
        var result = ActorDamage.Apply(target, 20, inflictor: new Actor { PierceArmor = pierce });
        Assert.Equal(lost, result.HealthLost);
        Assert.Equal(20 - lost, result.ArmorLost);
        Assert.Equal(armor, target is PlayerPawn pawn ? pawn.Inventory.Armor : target.Armor);
    }

    [Fact]
    public void SourceFlagDoesNotPierceWithoutInflictorFlag()
    {
        var target = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(10, ActorDamage.Apply(target, 20, source: new Actor { PierceArmor = true }, inflictor: new Actor()).HealthLost);
    }

    [Fact]
    public void QualifiedScriptFlagControlsArmorBypass()
    {
        var inflictor = new Actor();
        Assert.True(AcsActorFlags.TrySet(inflictor, "Actor.PIERCEARMOR", true));
        Assert.True(AcsActorFlags.TryGet(inflictor, "piercearmor", out var enabled));
        Assert.True(enabled);
        var target = new Actor { Health = 100, Armor = 100, ArmorSavePercent = 50 };
        Assert.Equal(20, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
        Assert.True(AcsActorFlags.TrySet(inflictor, "PIERCEARMOR", false));
        Assert.Equal(10, ActorDamage.Apply(target, 20, inflictor: inflictor).HealthLost);
    }
}
