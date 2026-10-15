namespace HCDE.MapLoader.Tests;

public class TerrainStepDistanceTests
{
    [Theory]
    [InlineData("stepdistance 32.125 stepdistanceminvel 0.1", 32.125, 0.1)]
    [InlineData("STEPDISTANCE -1 STEPDISTANCEMINVEL -2", -1, -2)]
    [InlineData("", 0, 0)]
    public void ParsesNativeDoubleFields(string fields, double distance, double minimumVelocity)
    {
        Assert.True(TerrainDefinitionParser.TryParse($"terrain Mud {{ {fields} }}", out var definitions, out var error), error);
        var terrain = Assert.Single(definitions);
        Assert.Equal(distance, terrain.StepDistance);
        Assert.Equal(minimumVelocity, terrain.StepDistanceMinVel);
    }

    [Fact]
    public void ModifyAcrossLumpsAndCopiesPreserveFieldsWhileResetClearsThem()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Mud { stepdistance 32.125 stepdistanceminvel 0.1 }",
            out var first, out var error), error);
        Assert.True(TerrainDefinitionParser.TryParseData("terrain mud modify { footclip 4 }", out var modified, out error, first), error);
        var copy = new PlayLevel { TerrainDefinitions = modified.Definitions }.CopyForSimulation();
        var terrain = Assert.Single(copy.TerrainDefinitions);
        Assert.Equal(32.125, terrain.StepDistance);
        Assert.Equal(0.1, terrain.StepDistanceMinVel);
        Assert.Equal(4, terrain.FootClip);
        Assert.True(TerrainDefinitionParser.TryParseData("terrain mud { }", out var reset, out error, modified), error);
        Assert.Equal(new LevelTerrainDefinition("Mud"), Assert.Single(reset.Definitions));
    }

    [Theory]
    [InlineData("stepdistance NaN")]
    [InlineData("stepdistance Infinity")]
    [InlineData("stepdistance 1e999")]
    [InlineData("stepdistanceminvel NaN")]
    [InlineData("stepdistanceminvel word")]
    [InlineData("stepdistanceminvel")]
    public void InvalidValuesFailExplicitly(string fields)
    {
        Assert.False(TerrainDefinitionParser.TryParse($"terrain Mud {{ {fields} }}", out _, out var error));
        Assert.NotNull(error);
    }
}
