using HCDE.MapLoader;

namespace HCDE.Playsim.Tests;

public class DamageTypeMapInfoTests
{
    [Fact]
    public void StartupLoadsLevelDamageDefinitions()
    {
        var level = new PlayLevel
        {
            Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }],
            DamageTypes = [new HCDE.Gamedata.MapInfoDamageType("Acid", 0.5, true, true)],
        };
        var sim = AuthoritySimulation.Start(level.CopyForSimulation());
        var player = sim.Players.Single(); player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        Assert.Equal(10, ActorDamage.Apply(player, 20, damageType: "Acid").HealthLost);
        Assert.Equal(100, player.Inventory.Armor);
    }
    [Fact]
    public void ParsedDefinitionControlsFactorAndArmor()
    {
        var sim = AuthoritySimulation.Start(new PlayLevel
        { Sectors = [new LevelSector { CeilingHeight = 128 }], Things = [new LevelThing { Type = 1 }] });
        Assert.True(sim.DamageTypes.TryLoadMapInfo("DamageType \"Acid\" { Factor = 0.5 ReplaceFactor NoArmor Obituary = \"acid death\" }", out var error), error);
        var player = sim.Players.Single(); player.Inventory.Armor = 100; player.Inventory.ArmorSavePercent = 50;
        player.SetDamageFactor("None", 0.5);
        Assert.Equal(10, ActorDamage.Apply(player, 20, damageType: "acid").HealthLost);
        Assert.Equal(100, player.Inventory.Armor);
    }

    [Theory]
    [InlineData("DamageType Fire { Factor = nope }")]
    [InlineData("DamageType Fire { Unknown }")]
    [InlineData("DamageType Fire { NoArmor")]
    [InlineData("DamageType { }")]
    public void InvalidTextDoesNotPartiallyChangeCatalog(string text)
    {
        var catalog = new DamageTypeCatalog(); catalog.Define("Fire", 2);
        Assert.False(catalog.TryLoadMapInfo("DamageType Fire { Factor = 0 } " + text, out var error));
        Assert.NotNull(error);
        Assert.Equal(2, catalog.Find("Fire")!.Value.Factor);
    }

    [Fact]
    public void LaterDefinitionReplacesWholeEarlierDefinitionAndZeroSetsReplace()
    {
        Assert.True(HCDE.Gamedata.MapInfoParser.TryParse("DamageType Fire { NoArmor } DamageType fire { Factor = 0 }", out var set, out var error), error);
        var definition = Assert.Single(set.DamageTypes);
        Assert.True(definition.ReplaceFactor); Assert.False(definition.NoArmor);
    }
}
