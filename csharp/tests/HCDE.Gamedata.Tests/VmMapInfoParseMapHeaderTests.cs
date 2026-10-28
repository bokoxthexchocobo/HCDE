namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapHeaderTests
{
    [Fact]
    public void ResolveMapNameToken_OldHexenNumber()
    {
        var hexen = false;
        var name = VmMapInfoParseMapHeader.ResolveMapNameToken(true, 2, "", newFormat: false, ref hexen);
        Assert.Equal("MAP02", name);
        Assert.True(hexen);
    }

    [Fact]
    public void ApplyLevelTitle_DollarPrefixSetsLookup()
    {
        var draft = new VmMapInfoHeaderDraft();
        VmMapInfoParseMapHeader.ApplyLevelTitle(draft, "$MYNAME", useLookupTable: false);
        Assert.True(draft.LookupLevelName);
        Assert.Equal("MYNAME", draft.LevelName);
    }

    [Fact]
    public void AssignDefaultLevelNum_ParsesMap01()
    {
        var draft = new VmMapInfoHeaderDraft { MapName = "MAP01" };
        VmMapInfoParseMapHeader.AssignDefaultLevelNum(draft);
        Assert.Equal(1, draft.LevelNum);
    }
}
