namespace HCDE.Gamedata.Tests;

public class VmUMapInfoStandardPropertyParserTests
{
    [Fact]
    public void TryApplyProperty_SetsLevelName()
    {
        var tokens = VmLockDefTokenize.Tokenize("= \"My Map\"");
        var i = 0;
        var entry = new VmUMapEntry();
        Assert.True(VmUMapInfoStandardPropertyParser.TryApplyProperty("levelname", tokens, ref i, entry, []));
        Assert.Equal("My Map", entry.LevelName);
    }
}
