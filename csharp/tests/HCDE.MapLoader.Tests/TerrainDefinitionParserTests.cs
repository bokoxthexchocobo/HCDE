namespace HCDE.MapLoader.Tests;

public class TerrainDefinitionParserTests
{
    [Theory]
    [InlineData(-100, 0, 32.0 / 65536)]
    [InlineData(100, 1, 602.3529411764706 / 65536)]
    [InlineData(0, 0xD001 / 65536.0, 32.0 / 65536)]
    public void FrictionUsesNativeClampingAndMovementFactor(double input, double friction, double moveFactor)
    {
        var text = "terrain Mud { friction " + input.ToString(System.Globalization.CultureInfo.InvariantCulture) + " }";
        Assert.True(TerrainDefinitionParser.TryParse(text, out var definitions, out var error), error);
        var definition = Assert.Single(definitions);
        Assert.Equal(friction, definition.Friction, 10); Assert.Equal(moveFactor, definition.MoveFactor, 10);
        Assert.True(TerrainDefinitionParser.TryParse("terrain Mud modify { }", out definitions, out error, definitions), error);
        Assert.Equal(definition, Assert.Single(definitions));
        Assert.True(TerrainDefinitionParser.TryParse("terrain Mud { }", out definitions, out error, definitions), error);
        Assert.Equal(0, Assert.Single(definitions).Friction); Assert.Equal(0, Assert.Single(definitions).MoveFactor);
    }

    [Theory]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    [InlineData("nope")]
    public void InvalidFrictionFails(string value)
        => Assert.False(TerrainDefinitionParser.TryParse("terrain Mud { friction " + value + " }", out _, out _));

    [Fact]
    public void SplashAndLandingMetadataSurviveModifyAndReset()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("splash S { } terrain Hot { splash S damageonland }", out var data, out var error), error);
        Assert.True(Assert.Single(data.Definitions).DamageOnLand); Assert.Equal("S", Assert.Single(data.Definitions).Splash);
        Assert.True(TerrainDefinitionParser.TryParseData("splash S modify { } terrain Hot modify { }", out data, out error, data), error);
        Assert.True(Assert.Single(data.Definitions).DamageOnLand);
        Assert.True(TerrainDefinitionParser.TryParseData("terrain Hot { }", out data, out error, data), error);
        Assert.False(Assert.Single(data.Definitions).DamageOnLand); Assert.Equal("", Assert.Single(data.Definitions).Splash);
    }

    [Fact]
    public void DamageFieldsHaveNativeDefaultsAndModifyVersusResetSemantics()
    {
        Assert.True(TerrainDefinitionParser.TryParse("terrain Hot { damageamount 7 damagetimemask 3 damagetype Fire } terrain Hot modify { damagetype Ice }", out var definitions, out var error), error);
        Assert.Equal(new LevelTerrainDefinition("Hot", "Ice", 7, 3), Assert.Single(definitions));
        Assert.True(TerrainDefinitionParser.TryParse("terrain Hot { }", out definitions, out error, definitions), error);
        Assert.Equal(new LevelTerrainDefinition("Hot", "None", 0, 31), Assert.Single(definitions));
    }

    [Theory]
    [InlineData("damageamount nope")]
    [InlineData("damagetimemask -1")]
    [InlineData("damagetimemask 2147483648")]
    public void InvalidDamageFieldsFail(string property)
    {
        Assert.False(TerrainDefinitionParser.TryParse("terrain Hot { " + property + " }", out _, out var error));
        Assert.NotNull(error);
    }

    [Theory]
    [InlineData("terrain Hot { damagetype Lava }", "Fire")]
    [InlineData("terrain Hot { damagetype Ice } terrain hot modify { }", "Ice")]
    [InlineData("terrain Hot { damagetype Ice } terrain Hot { }", "None")]
    [InlineData("/* note */ terrain \"Hot\" { // note\n damagetype \"Fire\" }", "Fire")]
    public void NativeDamageTypeAliasAndRedefinitionSemantics(string text, string expected)
    {
        Assert.True(TerrainDefinitionParser.TryParse(text, out var definitions, out var error), error);
        Assert.Equal(expected, Assert.Single(definitions).DamageType);
    }

    [Theory]
    [InlineData("terrain Hot { damagetype }")]
    [InlineData("terrain Hot { damagetype Fire")]
    [InlineData("terrain Hot { unknown 1 }")]
    [InlineData("terrain Hot { damagetype \"Fire }")]
    [InlineData("/* unfinished")]
    [InlineData("floor FLAT")]
    public void InvalidOrUnsupportedTextFailsWithoutPartialDefinitions(string text)
    {
        var prior = new[] { new LevelTerrainDefinition("Existing", "Ice") };
        Assert.False(TerrainDefinitionParser.TryParse("terrain New { } " + text, out var definitions, out var error, prior));
        Assert.Empty(definitions); Assert.NotNull(error); Assert.Equal("Ice", prior[0].DamageType);
    }

    [Fact]
    public void ModifyUsesDefinitionsFromEarlierLump()
    {
        Assert.True(TerrainDefinitionParser.TryParse("terrain Hot { damagetype Fire }", out var earlier, out _));
        Assert.True(TerrainDefinitionParser.TryParse("terrain hot modify { } terrain Cold modify { damagetype Ice }", out var parsed, out var error, earlier), error);
        Assert.Equal("Fire", parsed.Single(d => d.Name.Equals("hot", StringComparison.OrdinalIgnoreCase)).DamageType);
        Assert.Equal("Ice", parsed.Single(d => d.Name == "Cold").DamageType);
    }
}
