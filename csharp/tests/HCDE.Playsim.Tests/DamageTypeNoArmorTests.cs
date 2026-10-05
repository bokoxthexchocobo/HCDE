using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageTypeNoArmorTests
{
    [Theory]
    [InlineData(false, false, 10)]
    [InlineData(false, true, 20)]
    [InlineData(true, false, 10)]
    [InlineData(true, true, 20)]
    public void DefinitionControlsPlayerAndMonsterArmor(bool monster, bool noArmor, int expected)
    {
        var sim = Room(); var actor = monster ? sim.Actors.Last() : sim.Players.Single();
        actor.Health = 100;
        if (actor is PlayerPawn player) { player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50; }
        else { actor.Armor = 100; actor.ArmorSavePercent = 50; }
        sim.DamageTypes.Define("Acid", noArmor: noArmor);
        Assert.Equal(expected, ActorDamage.Apply(actor, 20, damageType: "aCiD").HealthLost);
        Assert.Equal(noArmor ? 100 : 90, actor is PlayerPawn pawn ? pawn.Inventory.Armor : actor.Armor);
    }

    [Fact]
    public void BuiltInDrowningCanBeRedefined()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        Assert.Equal(20, ActorDamage.Apply(player, 20, damageType: "Drowning").HealthLost);
        sim.DamageTypes.Define("Drowning", noArmor: false);
        Assert.Equal(10, ActorDamage.Apply(player, 20, damageType: "Drowning").HealthLost);
    }

    [Fact]
    public void NoArmorDoesNotBypassDamageFactor()
    {
        var sim = Room(); var player = sim.Players.Single();
        player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        sim.DamageTypes.Define("Acid", 0.5, noArmor: true);
        Assert.Equal(10, ActorDamage.Apply(player, 20, damageType: "Acid").HealthLost);
        Assert.Equal(100, player.Inventory.Armor);
    }

    private static AuthoritySimulation Room() => AuthoritySimulation.Start(new PlayLevel
    {
        Sectors = [new LevelSector { CeilingHeight = 128 }],
        Things = [new LevelThing { Type = 1 }, new LevelThing { Type = 3004, X = 400 }],
    });
}
