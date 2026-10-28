namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapHeader1035SliceTests
{
    [Fact]
    public void GetDefaultLevelNum_ParsesMap01()
    {
        var num = VmMapInfoParseMapHeader1035Slice.GetDefaultLevelNum("MAP01", out var id24);
        Assert.Equal(1, num);
        Assert.Equal(1, id24);
    }

    [Fact]
    public void GetDefaultLevelNum_E1M2_ReturnsEpisodeFormula()
    {
        var num = VmMapInfoParseMapHeader1035Slice.GetDefaultLevelNum("E1M2", out var id24);
        Assert.Equal(2, num);
        Assert.Equal(2, id24);
    }

    [Fact]
    public void GetDefaultLevelNum_E1M10_SetsId24Only()
    {
        var num = VmMapInfoParseMapHeader1035Slice.GetDefaultLevelNum("E1M10", out var id24);
        Assert.Equal(0, num);
        Assert.Equal(10, id24);
    }

    [Fact]
    public void ResolveMapNameToken_OldFormatNumberSetsHexenHack()
    {
        var hexen = false;
        var format = VmMapInfoParseMapHeader1035Slice.FmtNew;
        var name = VmMapInfoParseMapHeader1035Slice.ResolveMapNameToken(
            tokenIsNumber: true,
            number: 3,
            text: "",
            formatType: 0,
            ref hexen,
            ref format);
        Assert.Equal("MAP03", name);
        Assert.True(hexen);
        Assert.Equal(0, format);
    }

    [Fact]
    public void ApplyHexenHackDefaults_ClearsWallLightAndSetsFlags()
    {
        var level = new VmMapInfoLevelFlags();
        var h = 5;
        var v = 7;
        VmMapInfoParseMapHeader1035Slice.ApplyHexenHackDefaults(level, ref h, ref v);
        Assert.Equal(0, h);
        Assert.Equal(0, v);
        Assert.Equal(VmMapInfoParseMapHeader1035Slice.HexenHackFlags, level.Flags);
        Assert.Equal(VmMapInfoParseMapHeader1035Slice.HexenHackFlags2, level.Flags2);
    }

    [Fact]
    public void TryApplyDollarLevelTitle_SetsLookup()
    {
        var draft = new VmMapInfoHeaderDraft();
        Assert.True(VmMapInfoParseMapHeader1035Slice.TryApplyDollarLevelTitle(draft, "$FOO"));
        Assert.True(draft.LookupLevelName);
        Assert.Equal("FOO", draft.LevelName);
    }

    [Fact]
    public void TryApplyLookupKeywordTitle_RequiresSecondToken()
    {
        var draft = new VmMapInfoHeaderDraft();
        Assert.True(VmMapInfoParseMapHeader1035Slice.TryApplyLookupKeywordTitle(draft, "lookup", "BAR"));
        Assert.Equal("BAR", draft.LevelName);
    }

    [Fact]
    public void TryApplyHexenWadTitleKey_UsesTxtKey()
    {
        var draft = new VmMapInfoHeaderDraft();
        Assert.True(VmMapInfoParseMapHeader1035Slice.TryApplyHexenWadTitleKey(
            draft,
            hexenHack: true,
            "HEXEN.WAD",
            "MAP01",
            key => key.StartsWith("TXT_HEXEN", StringComparison.Ordinal)));
        Assert.True(draft.LookupLevelName);
    }

    [Fact]
    public void ShouldExitMapDefinitionAfterCloseBrace_MirrorsElseBreak()
    {
        Assert.True(VmMapInfoParseMapHeader1035Slice.ShouldExitMapDefinitionAfterCloseBrace(true));
        Assert.False(VmMapInfoParseMapHeader1035Slice.ShouldExitMapDefinitionAfterCloseBrace(false));
    }
}
