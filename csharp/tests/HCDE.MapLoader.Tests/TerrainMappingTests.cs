namespace HCDE.MapLoader.Tests;

public class TerrainMappingTests
{
    [Theory]
    [InlineData("floor FLAT Hot", "Fire")]
    [InlineData("floor optional FLAT Hot", "Fire")]
    [InlineData("floor FLAT None", "Ice")]
    [InlineData("floor FLAT Null", "Ice")]
    [InlineData("floor FLAT Unknown", "Ice")]
    [InlineData("floor FLAT Hot floor flat Cold", "Ice")]
    [InlineData("", "Ice")]
    public void MappingAndDefaultResolveNativeFallback(string mapping, string expected)
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Hot { damagetype Fire } terrain Cold { damagetype Ice } defaultterrain Cold " + mapping,
            out var data, out var error), error);
        var level = new PlayLevel { TerrainDefinitions = data.Definitions, FloorTerrainMappings = data.Floors,
            DefaultTerrain = data.DefaultTerrain, Sectors = [new LevelSector { FloorPic = "flat" }] };
        Assert.Equal(expected, level.FloorTerrainDamageType(0));
        Assert.Equal(expected, level.CopyForSimulation().FloorTerrainDamageType(0));
        level.Sectors[0].FloorTerrain = "Hot"; Assert.Equal("Fire", level.FloorTerrainDamageType(0));
    }

    [Fact]
    public void LaterLumpPreservesDefaultAndReplacesMapping()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Hot { damagetype Fire } defaultterrain Hot floor FLAT Hot", out var prior, out _));
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Cold { damagetype Ice } floor flat Cold", out var data, out var error, prior), error);
        Assert.Equal("Hot", data.DefaultTerrain); Assert.Equal("Cold", Assert.Single(data.Floors).Terrain);
    }

    [Theory]
    [InlineData("defaultterrain")]
    [InlineData("floor optional FLAT")]
    public void MissingArgumentsFail(string text)
    {
        Assert.False(TerrainDefinitionParser.TryParseData(text, out _, out var error)); Assert.NotNull(error);
    }
}
