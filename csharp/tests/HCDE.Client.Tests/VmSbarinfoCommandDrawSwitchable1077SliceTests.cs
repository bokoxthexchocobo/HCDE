namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawSwitchable1077SliceTests
{
    [Fact]
    public void MergeInventoryDualConditionDrawAlt_MatchesNativeTable()
    {
        Assert.Equal(3, VmSbarinfoCommandDrawSwitchable1077Slice.MergeInventoryDualConditionDrawAlt(
            drawAlt: 1, conditionAnd: true, secondConditionMet: true));
        Assert.Equal(2, VmSbarinfoCommandDrawSwitchable1077Slice.MergeInventoryDualConditionDrawAlt(
            0, true, false));
    }

    [Fact]
    public void ResolveInventoryDrawAltWhenFirstFails_UsesOffStateWhenMissing()
    {
        Assert.Equal(1, VmSbarinfoCommandDrawSwitchable1077Slice.ResolveInventoryDrawAltWhenFirstFails(false));
    }

    [Fact]
    public void EvaluateInventoryAmountCondition_DelegatesTo1075Compare()
    {
        Assert.True(VmSbarinfoCommandDrawSwitchable1077Slice.EvaluateInventoryAmountCondition(
            itemPresent: true,
            VmSbarinfoCommandDrawSwitchable1075Slice.SwitchableCompareOperator.Greater,
            threshold: 1,
            amount: 5));
    }

    [Fact]
    public void ResolveSwitchableOffImageIndex_MapsDrawAltToImageSlot()
    {
        Assert.Equal(2, VmSbarinfoCommandDrawSwitchable1077Slice.ResolveSwitchableOffImageIndex(3));
    }
}

public class VmSbarinfoCommandDrawString1077SliceTests
{
    [Fact]
    public void ResolveDrawStringFontSource_FallsBackWhenLookupFails()
    {
        Assert.Equal(
            VmSbarinfoCommandDrawString1077Slice.DrawStringFontSource.SmallFallback,
            VmSbarinfoCommandDrawString1077Slice.ResolveDrawStringFontSource(fontLookupSucceeded: false));
    }

    [Fact]
    public void TryParseDrawStringAlignment_MapsRightLeftCenter()
    {
        var (ok, align, invalid) = VmSbarinfoCommandDrawString1077Slice.TryParseDrawStringAlignment("left");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawString1077Slice.DrawStringAlignment.Left, align);
        Assert.False(invalid);
    }

    [Fact]
    public void ApplyDrawShadowFlag_EnablesShadowWithOptionalOffsets()
    {
        var (enabled, sx, sy) = VmSbarinfoCommandDrawString1077Slice.ApplyDrawShadowFlag(
            sawDrawShadowKeyword: true,
            hasParenthesizedOffsets: true,
            parsedShadowX: 4,
            parsedShadowY: 6,
            priorShadowX: 2,
            priorShadowY: 2);
        Assert.True(enabled);
        Assert.Equal(4, sx);
        Assert.Equal(6, sy);
    }

    [Fact]
    public void ApplyLineBreaksFlag_StoresBreakWidth()
    {
        var (enabled, width) = VmSbarinfoCommandDrawString1077Slice.ApplyLineBreaksFlag(true, 640);
        Assert.True(enabled);
        Assert.Equal(640, width);
    }

    [Fact]
    public void ComputeLineBreakDrawY_UsesHeightPlusFourSpacing()
    {
        Assert.Equal(14, VmSbarinfoCommandDrawString1077Slice.ComputeLineBreakDrawY(0, lineIndex: 1, fontHeight: 10));
    }

    [Fact]
    public void ClassifyDrawStringParseFlag_RejectsUnknownFlags()
    {
        var (_, unknown) = VmSbarinfoCommandDrawString1077Slice.ClassifyDrawStringParseFlag("notaflag");
        Assert.True(unknown);
    }
}
