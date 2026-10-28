namespace HCDE.Gamedata.Tests;

public class VmMapInfoParseMapInfo1030SliceTests
{
    [Fact]
    public void TryParseLateTopLevelKeyword_RejectsDamageTypeOnOldFormat()
    {
        Assert.False(VmMapInfoParseMapInfo1030Slice.TryParseLateTopLevelKeyword(
            "damagetype",
            VmMapInfoParseMapInfo1029Slice.FmtOld,
            out var cmd,
            out _,
            out var err,
            out _));
        Assert.Equal(VmMapInfoParseMapInfo1030Slice.LateTopLevelCommand.None, cmd);
        Assert.Contains("damagetype", err);
    }

    [Fact]
    public void TryParseLateTopLevelKeyword_AcceptsSpawnNumsOnNewFormat()
    {
        Assert.True(VmMapInfoParseMapInfo1030Slice.TryParseLateTopLevelKeyword(
            "spawnnums",
            VmMapInfoParseMapInfo1029Slice.FmtNew,
            out var cmd,
            out _,
            out _,
            out _));
        Assert.Equal(VmMapInfoParseMapInfo1030Slice.LateTopLevelCommand.SpawnNums, cmd);
    }

    [Fact]
    public void TryParseLateTopLevelKeyword_DetectsAutomapOverlay()
    {
        Assert.True(VmMapInfoParseMapInfo1030Slice.TryParseLateTopLevelKeyword(
            "automap_overlay",
            VmMapInfoParseMapInfo1029Slice.FmtNew,
            out var cmd,
            out var overlay,
            out _,
            out _));
        Assert.Equal(VmMapInfoParseMapInfo1030Slice.LateTopLevelCommand.Automap, cmd);
        Assert.True(overlay);
    }

    [Fact]
    public void FormatUnknownTopLevelKeyword_MatchesNativeShape()
    {
        Assert.Equal(
            "foo: Unknown top level keyword",
            VmMapInfoParseMapInfo1030Slice.FormatUnknownTopLevelKeyword("foo"));
    }

    [Fact]
    public void CreateClearedMapinfoState_MatchesNativeDefaults()
    {
        var snap = VmMapInfoParseMapInfo1030Slice.CreateClearedMapinfoState();
        Assert.Equal(-1, snap.DefaultSkill);
        Assert.Equal("", snap.PrimaryLevelF1Pic);
        Assert.True(snap.PrimaryLevelInfoCleared);
    }

    [Fact]
    public void AdvanceIndexedPropertyBracketDepth_TracksNestedBrackets()
    {
        var depth = 0;
        depth = VmMapInfoParseMapInfo1030Slice.AdvanceIndexedPropertyBracketDepth(depth, '[');
        depth = VmMapInfoParseMapInfo1030Slice.AdvanceIndexedPropertyBracketDepth(depth, '[');
        depth = VmMapInfoParseMapInfo1030Slice.AdvanceIndexedPropertyBracketDepth(depth, ']');
        Assert.Equal(1, depth);
    }

    [Fact]
    public void TryValidateCmpgninfMapNameToken_RejectsInvalidToken()
    {
        Assert.False(VmMapInfoParseMapInfo1030Slice.TryValidateCmpgninfMapNameToken(0, out var err));
        Assert.Equal(VmMapInfoParseMapInfo1030Slice.CmpgninfMapNameTokenError, err);
    }

}
