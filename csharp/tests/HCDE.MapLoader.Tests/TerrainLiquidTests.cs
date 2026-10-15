namespace HCDE.MapLoader.Tests;

public class TerrainLiquidTests
{
    [Theory]
    [InlineData("terrain Water { liquid damageamount 3 }", true)]
    [InlineData("terrain Water { LIQUID } terrain water modify { damageamount 3 }", true)]
    [InlineData("terrain Water { liquid } terrain water { damageamount 3 }", false)]
    [InlineData("terrain Water { damageamount 3 }", false)]
    public void LiquidFlagFollowsNativeUnaryModifyAndResetRules(string text, bool liquid)
    {
        Assert.True(TerrainDefinitionParser.TryParse(text, out var definitions, out var error), error);
        var definition = Assert.Single(definitions);
        Assert.Equal(liquid, definition.IsLiquid);
        Assert.Equal(3, definition.DamageAmount);
        Assert.Equal("Water", definition.Name);
    }

    [Fact]
    public void LiquidAndOriginalNameSurviveLaterLumpAndSimulationCopy()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Water { liquid }", out var first, out var error), error);
        Assert.True(TerrainDefinitionParser.TryParseData("terrain WATER modify { footclip 4 } floor FLAT Water",
            out var data, out error, first), error);
        var level = new PlayLevel
        {
            TerrainDefinitions = data.Definitions, FloorTerrainMappings = data.Floors,
            Sectors = [new LevelSector { FloorPic = "FLAT" }],
        };
        var terrain = Assert.IsType<LevelTerrainDefinition>(level.CopyForSimulation().FloorTerrainDefinition(0));
        Assert.True(terrain.IsLiquid);
        Assert.Equal("Water", terrain.Name);
        Assert.Equal(4, terrain.FootClip);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void LiquidDoesNotAcceptAnExplicitBoolean(string value)
        => Assert.False(TerrainDefinitionParser.TryParse($"terrain Water {{ liquid {value} }}", out _, out _));

    [Fact]
    public void RedefiningBuiltInSolidPreservesItsNativeNameAndNeutralLiquidDefault()
    {
        Assert.True(TerrainDefinitionParser.TryParse("terrain solid { }", out var definitions, out var error), error);
        var terrain = Assert.Single(definitions);
        Assert.Equal("Solid", terrain.Name);
        Assert.False(terrain.IsLiquid);
    }
}
