namespace HCDE.MapLoader.Tests;

public class BuiltInSolidTerrainTests
{
    [Theory]
    [InlineData("")]
    [InlineData("floor FLAT Solid")]
    [InlineData("defaultterrain solid")]
    [InlineData("terrain Water { } defaultterrain Water floor FLAT Solid")]
    public void NeutralTerrainExistsWithoutAnExplicitDefinition(string text)
    {
        Assert.True(TerrainDefinitionParser.TryParseData(text, out var data, out var error), error);
        var level = new PlayLevel
        {
            TerrainDefinitions = data.Definitions, FloorTerrainMappings = data.Floors,
            DefaultTerrain = data.DefaultTerrain, Sectors = [new LevelSector { FloorPic = "FLAT" }],
        };
        var terrain = Assert.IsType<LevelTerrainDefinition>(level.FloorTerrainDefinition(0));
        Assert.Equal("Solid", terrain.Name);
        Assert.Equal(0, terrain.DamageAmount);
        Assert.Equal(31, terrain.DamageTimeMask);
        Assert.Equal(0, terrain.FootClip);
        Assert.Equal(terrain, level.CopyForSimulation().FloorTerrainDefinition(0));
        Assert.Null(level.FloorTerrainDefinition(-1));
    }

    [Fact]
    public void ExplicitSolidDefinitionOverridesBuiltIn()
    {
        var level = new PlayLevel
        {
            TerrainDefinitions = [new("SOLID", DamageAmount: 3)],
            Sectors = [new LevelSector()],
        };
        Assert.Equal(3, level.FloorTerrainDefinition(0)?.DamageAmount);
        Assert.Equal("SOLID", level.FindTerrainDefinition("solid")?.Name);
    }
}
