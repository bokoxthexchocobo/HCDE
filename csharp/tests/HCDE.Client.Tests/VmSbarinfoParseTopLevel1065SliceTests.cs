namespace HCDE.Client.Tests;

public class VmSbarinfoParseTopLevel1065SliceTests
{
    [Fact]
    public void ClassifyBaseGameName_RecognizesStandardAndOverrideNames()
    {
        Assert.Equal(VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.Doom,
            VmSbarinfoParseTopLevel1065Slice.ClassifyBaseGameName("Doom"));
        Assert.Equal(VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.Strife,
            VmSbarinfoParseTopLevel1065Slice.ClassifyBaseGameName("strife"));
        Assert.Equal(VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.Unknown,
            VmSbarinfoParseTopLevel1065Slice.ClassifyBaseGameName("bogus"));
    }

    [Fact]
    public void StandardBaseRelativePath_MapsDoomHereticHexen()
    {
        Assert.Equal("sbarinfo/doom.txt",
            VmSbarinfoParseTopLevel1065Slice.StandardBaseRelativePath(
                VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.Doom));
        Assert.Null(VmSbarinfoParseTopLevel1065Slice.StandardBaseRelativePath(
            VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.Strife));
    }

    [Fact]
    public void ResolveGameTypeOverride_MatchesNativeStrifeAndNone()
    {
        Assert.Equal(8, VmSbarinfoParseTopLevel1065Slice.ResolveGameTypeOverride(
            VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.Strife));
        Assert.Equal(0, VmSbarinfoParseTopLevel1065Slice.ResolveGameTypeOverride(
            VmSbarinfoParseTopLevel1065Slice.BaseGameNameKind.None));
    }

    [Fact]
    public void ShouldLoadStandardBaseAfterLookup_UsesUnsetSentinel()
    {
        Assert.False(VmSbarinfoParseTopLevel1065Slice.ShouldLoadStandardBaseAfterLookup(-2));
        Assert.True(VmSbarinfoParseTopLevel1065Slice.ShouldLoadStandardBaseAfterLookup(3));
    }

    [Fact]
    public void BuildInterpolateHealthState_HonorsTrueAndOptionalSpeed()
    {
        var (enabled, hasSpeed, speed) = VmSbarinfoParseTopLevel1065Slice.BuildInterpolateHealthState(
            checkTrueHit: true, validatedFalseToken: false, hasCommaSpeed: true, speedValue: 12);
        Assert.True(enabled);
        Assert.True(hasSpeed);
        Assert.Equal(12, speed);
    }

    [Fact]
    public void ResolveTopLevelBool_AcceptsFalsePath()
    {
        Assert.False(VmSbarinfoParseTopLevel1065Slice.ResolveTopLevelBool(false, validatedFalseToken: true));
    }

    [Fact]
    public void MergeStatusBarHeight_ReplacesWhenIntPresent()
    {
        Assert.Equal(48, VmSbarinfoParseTopLevel1065Slice.MergeStatusBarHeight(32, 48, parsedIntPresent: true));
        Assert.Equal(32, VmSbarinfoParseTopLevel1065Slice.MergeStatusBarHeight(32, 48, false));
    }

    [Fact]
    public void ResolveMonospaceSpacingCharacter_ClearsOnFalseBranch()
    {
        Assert.Equal(' ', VmSbarinfoParseTopLevel1065Slice.ResolveMonospaceSpacingCharacter(true, ' '));
        Assert.Equal('\0', VmSbarinfoParseTopLevel1065Slice.ResolveMonospaceSpacingCharacter(false, 'x'));
    }

    [Fact]
    public void TryClassifySpacingAlignment_RecognizesNativeKeywords()
    {
        Assert.Equal(VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Center,
            VmSbarinfoParseTopLevel1065Slice.TryClassifySpacingAlignment("center"));
        Assert.Null(VmSbarinfoParseTopLevel1065Slice.TryClassifySpacingAlignment("bogus"));
    }

    [Fact]
    public void ResolveCustomSlotAction_DelegatesTo1064()
    {
        Assert.Equal(
            VmSbarinfoMainBlockLoad1064Slice.DefaultScriptAction.ParseExisting,
            VmSbarinfoParseTopLevel1065Slice.ResolveCustomSlotAction(slotIsNull: false));
    }

    [Fact]
    public void ShouldRecurseParsedInclude_RequiresValidLump()
    {
        Assert.True(VmSbarinfoParseTopLevel1065Slice.ShouldRecurseParsedInclude(true, 2));
        Assert.False(VmSbarinfoParseTopLevel1065Slice.ShouldRecurseParsedInclude(true, -1));
    }

    [Fact]
    public void TryParseOptionalSpacingAlignment_SkipsWhenNoComma()
    {
        Assert.Null(VmSbarinfoParseTopLevel1065Slice.TryParseOptionalSpacingAlignment(false, "left"));
        Assert.Equal(VmSbarinfoParseTopLevel1065Slice.SpacingAlignment.Left,
            VmSbarinfoParseTopLevel1065Slice.TryParseOptionalSpacingAlignment(true, "left"));
    }

    [Fact]
    public void ShouldContinueCustomLumpScan_StopsOnMissingLump()
    {
        Assert.True(VmSbarinfoParseTopLevel1065Slice.ShouldContinueCustomLumpScan(1));
        Assert.False(VmSbarinfoParseTopLevel1065Slice.ShouldContinueCustomLumpScan(-1));
    }
}
