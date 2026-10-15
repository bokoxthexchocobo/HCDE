namespace HCDE.MapLoader.Tests;

public class TerrainOverrideFallbackTests
{
    [Theory]
    [InlineData("None", "Water")]
    [InlineData("nUlL", "Water")]
    [InlineData("", "Water")]
    [InlineData("Mud", "Mud")]
    public void MissingOverrideUsesTextureMappingBeforeDefault(string terrainOverride, string expected)
    {
        var level = Level(terrainOverride, "Water");
        Assert.Equal(expected, level.FloorTerrainDefinition(0)?.Name);
        Assert.Equal(expected, level.CopyForSimulation().FloorTerrainDefinition(0)?.Name);
    }

    [Theory]
    [InlineData("None")]
    [InlineData("Null")]
    [InlineData("")]
    public void MissingTextureTerrainUsesDefaultAfterOverrideFallback(string mapping)
    {
        var level = Level("None", mapping);
        Assert.Equal("Solid", level.FloorTerrainDefinition(0)?.Name);
    }

    private static PlayLevel Level(string terrainOverride, string mapping) => new()
    {
        Sectors = [new LevelSector { FloorPic = "FLAT", FloorTerrain = terrainOverride }],
        TerrainDefinitions = [new("Solid"), new("Water"), new("Mud")],
        FloorTerrainMappings = [new("flat", mapping)],
        DefaultTerrain = "Solid",
    };
}
