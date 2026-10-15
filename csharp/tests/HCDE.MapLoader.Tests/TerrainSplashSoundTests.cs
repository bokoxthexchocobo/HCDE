namespace HCDE.MapLoader.Tests;

public class TerrainSplashSoundTests
{
    [Fact]
    public void SoundSurvivesModifyAndCopyAndClearsOnReset()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("splash S { SOUND \"water/splash\" }", out var first, out var error), error);
        Assert.True(TerrainDefinitionParser.TryParseData("splash s modify { }", out var modified, out error, first), error);
        var level = new PlayLevel { TerrainSplashDefinitions = modified.SplashDefinitions! };
        Assert.Equal("water/splash", Assert.Single(level.CopyForSimulation().TerrainSplashDefinitions).NormalSplashSound);
        Assert.True(TerrainDefinitionParser.TryParseData("splash s { }", out var reset, out error, modified), error);
        Assert.Equal("", Assert.Single(reset.SplashDefinitions!).NormalSplashSound);
    }

    [Theory]
    [InlineData("splash S { sound }")]
    [InlineData("splash S { sound sound")]
    [InlineData("splash S { unsupported sound }")]
    [InlineData("splash S { normalsound water/splash }")]
    [InlineData("splash S { smallsound }")]
    public void MalformedOrUnsupportedPropertiesFail(string text)
        => Assert.False(TerrainDefinitionParser.TryParseData(text, out _, out _));

    [Fact]
    public void SmallSoundSurvivesModifyAndClearsOnReset()
    {
        Assert.True(TerrainDefinitionParser.TryParseData("splash S { smallsound water/small }", out var first, out var error), error);
        Assert.True(TerrainDefinitionParser.TryParseData("splash S modify { sound water/normal }", out var modified, out error, first), error);
        Assert.Equal("water/small", Assert.Single(modified.SplashDefinitions!).SmallSplashSound);
        Assert.True(TerrainDefinitionParser.TryParseData("splash S { }", out var reset, out error, modified), error);
        Assert.Equal("", Assert.Single(reset.SplashDefinitions!).SmallSplashSound);
    }
}
