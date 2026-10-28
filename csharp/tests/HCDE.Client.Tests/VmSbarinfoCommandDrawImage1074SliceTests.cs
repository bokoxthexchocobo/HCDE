namespace HCDE.Client.Tests;

public class VmSbarinfoCommandDrawImage1074SliceTests
{
    [Fact]
    public void ApplyParsedMaxWidth_SetsDrawInBoxWhenPositive()
    {
        var (width, flags) = VmSbarinfoCommandDrawImage1074Slice.ApplyParsedMaxWidth(64, 0);
        Assert.Equal(64, width);
        Assert.True((flags & VmSbarinfoCommandDrawImage1073Slice.DiDrawInBox) != 0);
    }

    [Fact]
    public void ApplyParsedMaxHeight_ClearsToSentinelWhenNonPositive()
    {
        var (height, flags) = VmSbarinfoCommandDrawImage1074Slice.ApplyParsedMaxHeight(0, 0x10);
        Assert.Equal(-1, height);
        Assert.Equal(0x10u, flags);
    }

    [Fact]
    public void TryResolveDrawImageAlignment_MapsCenterKeywords()
    {
        var (ok, offset, invalid) =
            VmSbarinfoCommandDrawImage1074Slice.TryResolveDrawImageAlignment("centerbottom");
        Assert.True(ok);
        Assert.Equal(VmSbarinfoCommandDrawImage1074Slice.OffsetCenterBottom, offset);
        Assert.False(invalid);
    }

    [Fact]
    public void ApplyDrawImageParseFlag_ForceScaleRequiresDrawInBox()
    {
        var (withBox, _) = VmSbarinfoCommandDrawImage1074Slice.ApplyDrawImageParseFlag(
            "forcescale",
            VmSbarinfoCommandDrawImage1073Slice.DiDrawInBox);
        Assert.True((withBox & VmSbarinfoCommandDrawImage1073Slice.DiForceScale) != 0);

        var (withoutBox, unknown) = VmSbarinfoCommandDrawImage1074Slice.ApplyDrawImageParseFlag("forcescale", 0);
        Assert.False((withoutBox & VmSbarinfoCommandDrawImage1073Slice.DiForceScale) != 0);
        Assert.False(unknown);
    }

    [Fact]
    public void ApplyDrawImageParseFlag_ReportsUnknownKeywords()
    {
        var (_, unknown) = VmSbarinfoCommandDrawImage1074Slice.ApplyDrawImageParseFlag("notaflag", 0);
        Assert.True(unknown);
    }

    [Fact]
    public void ShouldDelegateParseToFlowControl_MatchesAlternateOnFail()
    {
        Assert.True(VmSbarinfoCommandDrawImage1074Slice.ShouldDelegateParseToFlowControl(
            VmSbarinfoCommandDrawImage1073Slice.DiAlternateOnFail));
    }

    [Fact]
    public void ResetTickDrawImageState_ClearsSpawnScaleWhenApplyScaleWasSet()
    {
        var (_, applyScale, sx, sy) = VmSbarinfoCommandDrawImage1074Slice.ResetTickDrawImageState(
            hadApplyScale: true,
            priorSpawnScaleX: 2,
            priorSpawnScaleY: 3);
        Assert.False(applyScale);
        Assert.Equal(1.0, sx);
        Assert.Equal(1.0, sy);
    }

    [Fact]
    public void ShouldResolveArmorTickIcon_RequiresNonZeroAmount()
    {
        Assert.True(VmSbarinfoCommandDrawImage1074Slice.ShouldResolveArmorTickIcon(true, 5));
        Assert.False(VmSbarinfoCommandDrawImage1074Slice.ShouldResolveArmorTickIcon(true, 0));
    }

    [Fact]
    public void CombineHexenArmorFillAlpha_ScalesBySlotRatio()
    {
        var alpha = VmSbarinfoCommandDrawImage1074Slice.CombineHexenArmorFillAlpha(1.0, 25, 100);
        Assert.Equal(0.25, alpha, 3);
    }

    [Fact]
    public void ResolveAlternateOnFailTruth_DetectsMissingOrInvalid()
    {
        Assert.True(VmSbarinfoCommandDrawImage1074Slice.ResolveAlternateOnFailTruth(true, false));
        Assert.True(VmSbarinfoCommandDrawImage1074Slice.ResolveAlternateOnFailTruth(false, true));
        Assert.False(VmSbarinfoCommandDrawImage1074Slice.ResolveAlternateOnFailTruth(false, false));
    }

    [Fact]
    public void ApplyInventoryIconSpawnScale_CopiesItemScaleWhenRequested()
    {
        var (apply, sx, sy) = VmSbarinfoCommandDrawImage1074Slice.ApplyInventoryIconSpawnScale(true, 1.5, 0.5);
        Assert.True(apply);
        Assert.Equal(1.5, sx);
        Assert.Equal(0.5, sy);
    }
}
