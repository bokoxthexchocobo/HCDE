namespace HCDE.MapLoader.Tests;

public class TerrainStepMetadataTests
{
    [Theory]
    [InlineData("terrain Mud { stepvolume 0.3 walksteptics 12 runsteptics 6 }")]
    [InlineData("terrain Mud { STEPVOLUME 0.3 WALKSTEPTICS 12 RUNSTEPTICS 6 } terrain Mud modify { }")]
    public void ParsesAndPreservesNativeStepFields(string text)
    {
        Assert.True(TerrainDefinitionParser.TryParse(text, out var definitions, out var error), error);
        var terrain = Assert.Single(definitions);
        Assert.Equal(0.3f, terrain.StepVolume);
        Assert.Equal(12, terrain.WalkStepTics);
        Assert.Equal(6, terrain.RunStepTics);
        var level = new PlayLevel { TerrainDefinitions = definitions };
        Assert.Equal(terrain, Assert.Single(level.CopyForSimulation().TerrainDefinitions));
    }

    [Fact]
    public void ResetClearsStepFieldsAndSignedIntegerValuesAreAccepted()
    {
        Assert.True(TerrainDefinitionParser.TryParse("terrain Mud { stepvolume -0.5 walksteptics -1 runsteptics -2 }",
            out var first, out var error), error);
        Assert.Equal(-1, Assert.Single(first).WalkStepTics);
        Assert.Equal(-2, Assert.Single(first).RunStepTics);
        Assert.Equal(-0.5f, Assert.Single(first).StepVolume);
        Assert.True(TerrainDefinitionParser.TryParse("terrain mud { }", out var reset, out error, first), error);
        Assert.Equal(new LevelTerrainDefinition("Mud"), Assert.Single(reset));
    }

    [Theory]
    [InlineData("stepvolume NaN")]
    [InlineData("stepvolume 1e100")]
    [InlineData("walksteptics 1.5")]
    [InlineData("runsteptics 2147483648")]
    [InlineData("stepvolume")]
    public void InvalidStepValuesFailParsing(string fields)
        => Assert.False(TerrainDefinitionParser.TryParse($"terrain Mud {{ {fields} }}", out _, out _));
}
