namespace HCDE.MapLoader.Tests;

public class SectorDamageMetadataTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void ContactFlagsSurviveLoadingEvenWithoutDamage(bool hurt, bool air)
    {
        var text = "namespace = \"ZDoom\"; sector { hurtmonsters = " + hurt.ToString().ToLowerInvariant()
            + "; harminair = " + air.ToString().ToLowerInvariant() + "; }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(hurt, sector.HurtMonsters); Assert.Equal(air, sector.HarmInAir);
        Assert.Equal(0, sector.DamageAmount);
    }

    [Theory]
    [InlineData("", 32)]
    [InlineData("damageinterval = 8;", 8)]
    [InlineData("damageinterval = 0;", 1)]
    [InlineData("damageinterval = -20;", 1)]
    [InlineData("damageinterval = 32768;", 1)]
    [InlineData("damageinterval = 65538;", 2)]
    public void LoadsDamagePropertiesAndNormalizesInterval(string interval, int expected)
    {
        var text = "namespace = \"ZDoom\"; sector { damageamount = -5; damagetype = \"Fire\"; leakiness = 256; " + interval + " }";
        Assert.True(UdmfTextMapParser.TryParse(text, out var map, out var error), error);
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(-5, sector.DamageAmount);
        Assert.Equal("Fire", sector.DamageType);
        Assert.Equal(expected, sector.DamageInterval);
        Assert.Equal(256, sector.Leakiness);
    }

    [Theory]
    [InlineData(32768, -32768)]
    [InlineData(65538, 2)]
    public void LeakinessUsesNativeSignedFieldWidth(int value, int expected)
    {
        Assert.True(UdmfTextMapParser.TryParse("namespace = \"ZDoom\"; sector { damageamount = 5; leakiness = " + value + "; }",
            out var map, out var error), error);
        Assert.Equal(expected, Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors).Leakiness);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void ZeroDamageClearsRelatedPropertiesWhenBuildingLevel(int amount)
    {
        var map = new UdmfTextMap { Sectors = [new UdmfSector
            { DamageAmount = amount, DamageType = "Fire", DamageInterval = 8, Leakiness = 128 }] };
        var sector = Assert.Single(LevelBuilder.FromUdmf(map, "MAP01").Sectors);
        Assert.Equal(amount, sector.DamageAmount);
        Assert.Equal(amount == 0 ? "None" : "Fire", sector.DamageType);
        Assert.Equal(amount == 0 ? 0 : 8, sector.DamageInterval);
        Assert.Equal(amount == 0 ? 0 : 128, sector.Leakiness);
    }
}
