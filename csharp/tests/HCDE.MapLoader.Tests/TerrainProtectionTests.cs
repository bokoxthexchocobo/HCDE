namespace HCDE.MapLoader.Tests;

public class TerrainProtectionTests
{
    [Theory]
    [InlineData("allowprotection")]
    [InlineData("AllowProtection")]
    public void ProtectionIsUnaryAndDoesNotConsumeNextProperty(string keyword)
    {
        Assert.True(TerrainDefinitionParser.TryParse($"terrain Lava {{ {keyword} damageamount 5 }}",
            out var definitions, out var error), error);
        Assert.True(definitions.Single().AllowProtection);
        Assert.Equal(5, definitions.Single().DamageAmount);
    }

    [Theory]
    [InlineData("terrain Lava { }")]
    [InlineData("terrain Lava { allowprotection } terrain Lava { damageamount 2 }")]
    public void FreshDefinitionDefaultsToUnprotected(string text)
    {
        Assert.True(TerrainDefinitionParser.TryParse(text, out var definitions, out var error), error);
        Assert.False(definitions.Single().AllowProtection);
    }

    [Fact]
    public void ModifyAcrossLumpsAndSimulationCopyPreserveProtection()
    {
        Assert.True(TerrainDefinitionParser.TryParse("terrain Lava { allowprotection }", out var first, out var error), error);
        Assert.True(TerrainDefinitionParser.TryParse("terrain Lava modify { damageamount 7 }", out var modified,
            out error, first), error);
        var level = new PlayLevel { TerrainDefinitions = modified, DefaultTerrain = "Lava" };
        var copied = level.CopyForSimulation();
        Assert.True(copied.TerrainDefinitions.Single().AllowProtection);
        Assert.Equal(7, copied.TerrainDefinitions.Single().DamageAmount);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public void ExplicitBooleanValueIsNotNativeSyntax(string value)
        => Assert.False(TerrainDefinitionParser.TryParse($"terrain Lava {{ allowprotection {value} }}", out _, out _));
}
