namespace HCDE.Gamedata.Tests;

public class GameInfoDropStyleTests
{
    [Theory]
    [InlineData("GameInfo { DefaultDropStyle = 2 }", 2)]
    [InlineData("gameinfo { defaultdropstyle = 1 } GameInfo { DefaultDropStyle = 2 }", 2)]
    [InlineData("GameInfo { DefaultDropStyle = 2 } GameInfo { titlepage = \"TITLEPIC\" }", 2)]
    [InlineData("GameInfo { DefaultDropStyle = -1 }", -1)]
    public void ReadsIntegerAndPreservesEarlierValueAcrossBlocks(string text, int expected)
    {
        Assert.True(MapInfoParser.TryParse(text, out var info, out var error), error);
        Assert.Equal(expected, info.DefaultDropStyle);
    }

    [Theory]
    [InlineData("GameInfo { DefaultDropStyle = nope }")]
    [InlineData("GameInfo { DefaultDropStyle = 2")]
    [InlineData("GameInfo { DefaultDropStyle 2 }")]
    [InlineData("GameInfo { intro { nested { }")]
    public void MalformedBlocksFail(string text)
    {
        Assert.False(MapInfoParser.TryParse(text, out _, out var error));
        Assert.NotNull(error);
    }
}
