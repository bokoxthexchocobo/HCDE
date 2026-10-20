namespace HCDE.Gamedata.Tests;

public class VmGameInfoParseGameInfoTests
{
    [Fact]
    public void TryParseBlock_ReadsDialogue()
    {
        var tokens = VmLockDefTokenize.Tokenize("{ dialogue \"shop\" }");
        var i = 0;
        var doc = new VmGameInfoDocument();
        Assert.True(VmGameInfoParseGameInfo.TryParseBlock(tokens, ref i, doc, []));
        Assert.Equal("shop", doc.Dialogue);
    }
}
