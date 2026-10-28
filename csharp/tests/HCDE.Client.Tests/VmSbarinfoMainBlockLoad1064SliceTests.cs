namespace HCDE.Client.Tests;

public class VmSbarinfoMainBlockLoad1064SliceTests
{
    [Fact]
    public void ApplyDrawAlpha_DelegatesTo1063Combine()
    {
        Assert.Equal(0.2, VmSbarinfoMainBlockLoad1064Slice.ApplyDrawAlpha(0.4, 0.5));
    }

    [Fact]
    public void ShouldParseFlowAfterFlagList_WhenSeparatorsEnd()
    {
        Assert.True(VmSbarinfoMainBlockLoad1064Slice.ShouldParseFlowAfterFlagList(false, false));
        Assert.False(VmSbarinfoMainBlockLoad1064Slice.ShouldParseFlowAfterFlagList(true, false));
        Assert.True(VmSbarinfoMainBlockLoad1064Slice.ShouldContinueMainBlockFlagTokens(true, false));
    }

    [Fact]
    public void StoreMainBlockAlpha_PreservesParsedFloat()
    {
        Assert.Equal(0.75, VmSbarinfoMainBlockLoad1064Slice.StoreMainBlockAlpha(0.75));
    }

    [Fact]
    public void ResolveScriptSlotAction_ChoosesCreateOrParse()
    {
        Assert.Equal(
            VmSbarinfoMainBlockLoad1064Slice.DefaultScriptAction.CreateNew,
            VmSbarinfoMainBlockLoad1064Slice.ResolveScriptSlotAction(slotIsNull: true));
        Assert.Equal(
            VmSbarinfoMainBlockLoad1064Slice.DefaultScriptAction.ParseExisting,
            VmSbarinfoMainBlockLoad1064Slice.ResolveScriptSlotAction(false));
    }

    [Fact]
    public void MatchTopLevelKeyword_FindsAppendStatusBar()
    {
        var idx = VmSbarinfoMainBlockLoad1064Slice.MatchTopLevelKeyword("appendstatusbar");
        Assert.True(VmSbarinfoMainBlockLoad1064Slice.TopLevelKeywordIsAppendStatusBar(idx));
    }

    [Fact]
    public void MatchStatusBarTypeName_RecognizesInventoryFullscreen()
    {
        Assert.Equal(5, VmSbarinfoMainBlockLoad1064Slice.MatchStatusBarTypeName("inventoryfullscreen"));
    }

    [Fact]
    public void ShouldLoadDefaultFromGameInfo_RequiresName()
    {
        Assert.False(VmSbarinfoMainBlockLoad1064Slice.ShouldLoadDefaultFromGameInfo(""));
        Assert.True(VmSbarinfoMainBlockLoad1064Slice.ShouldLoadDefaultFromGameInfo("STBAR"));
    }

    [Fact]
    public void ShouldEmitParseBanner_SkipsBatchRun()
    {
        Assert.False(VmSbarinfoMainBlockLoad1064Slice.ShouldEmitParseBanner(batchRun: true));
        Assert.True(VmSbarinfoMainBlockLoad1064Slice.ShouldEmitParseBanner(false));
    }

    [Fact]
    public void ScriptSlotConstants_MatchNativeHeaders()
    {
        Assert.Equal(0, VmSbarinfoMainBlockLoad1064Slice.CustomScriptSlot);
        Assert.Equal(1, VmSbarinfoMainBlockLoad1064Slice.DefaultScriptSlot);
        Assert.Equal(2, VmSbarinfoMainBlockLoad1064Slice.ScriptSlotCount);
    }
}
