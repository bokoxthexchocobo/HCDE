namespace HCDE.MapLoader.Tests;

public class TerrainStepSoundTests
{
    [Theory]
    [InlineData("stepsounds step/generic leftstepsounds step/left rightstepsounds step/right")]
    [InlineData("STEPSOUNDS \"step/generic\" LEFTSTEPSOUNDS \"step/left\" RIGHTSTEPSOUNDS \"step/right\"")]
    public void ParsesAllThreeSoundNamesWithoutConsumingFollowingProperties(string fields)
    {
        Assert.True(TerrainDefinitionParser.TryParse($"terrain Mud {{ {fields} stepvolume 0.5 }}",
            out var definitions, out var error), error);
        var terrain = Assert.Single(definitions);
        Assert.Equal("step/generic", terrain.StepSound);
        Assert.Equal("step/left", terrain.LeftStepSound);
        Assert.Equal("step/right", terrain.RightStepSound);
        Assert.Equal(0.5f, terrain.StepVolume);
    }

    [Fact]
    public void ModifyAndCopyPreserveNamesAndFullResetClearsThem()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Mud { stepsounds generic leftstepsounds left rightstepsounds right }",
            out var first, out var error), error);
        Assert.True(TerrainDefinitionParser.TryParseData("terrain mud modify { rightstepsounds replacement }",
            out var modified, out error, first), error);
        var copy = new PlayLevel { TerrainDefinitions = modified.Definitions }.CopyForSimulation();
        var terrain = Assert.Single(copy.TerrainDefinitions);
        Assert.Equal("generic", terrain.StepSound);
        Assert.Equal("left", terrain.LeftStepSound);
        Assert.Equal("replacement", terrain.RightStepSound);
        Assert.True(TerrainDefinitionParser.TryParseData("terrain mud { }", out var reset, out error, modified), error);
        Assert.Equal(new LevelTerrainDefinition("Mud"), Assert.Single(reset.Definitions));
    }

    [Theory]
    [InlineData("stepsounds")]
    [InlineData("leftstepsounds")]
    [InlineData("rightstepsounds")]
    public void MissingSoundNameFailsExplicitly(string property)
    {
        Assert.False(TerrainDefinitionParser.TryParse($"terrain Mud {{ {property} }}", out _, out var error));
        Assert.NotNull(error);
    }
}
