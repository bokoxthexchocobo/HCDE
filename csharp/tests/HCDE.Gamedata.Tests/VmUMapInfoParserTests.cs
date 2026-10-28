namespace HCDE.Gamedata.Tests;

public class VmUMapInfoParserTests
{
    [Fact]
    public void ResolveNextMapFromEndPic_MapsCast()
    {
        Assert.Contains("INTER_CAST", VmUMapInfoParser.ResolveNextMapFromEndPic("$CAST"));
    }

    [Fact]
    public void TryParseMapBlock_ReadsBossAction()
    {
        var tokens = VmLockDefTokenize.Tokenize("MAP01 { bossaction Imp , 11 , 1 }");
        var i = 0;
        var map = new VmUMapInfoMap();
        var id = 0;
        Assert.True(VmUMapInfoParser.TryParseMapBlock(tokens, ref i, map, [], ref id));
        Assert.Single(map.BossActions);
    }

    [Fact]
    public void CommitToLevelInfo_AppliesInterText()
    {
        var map = new VmUMapInfoMap
        {
            MapName = "MAP01",
            Entry = { InterText = "Hello", InterBackdrop = "SKY1" }
        };
        var target = new VmUMapLevelCommit();
        VmUMapInfoParser.CommitToLevelInfo(map, target, "D_INTER");
        Assert.Equal("Hello", target.InterText);
        Assert.Equal("D_INTER", target.InterMusic);
    }
}
