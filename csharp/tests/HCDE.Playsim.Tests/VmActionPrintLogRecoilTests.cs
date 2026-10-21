namespace HCDE.Playsim.Tests;

public class VmActionPrintLogRecoilTests
{
    [Fact]
    public void RecoilThrustAngleDegrees_AddsHalfTurn() =>
        Assert.Equal(270, VmActionPrintLogRecoil.RecoilThrustAngleDegrees(90));

    [Fact]
    public void ResolveDollarString_LooksUpWhenPrefixed() =>
        Assert.Equal("PICKUP", VmActionPrintLogRecoil.ResolveDollarString("$PICKUP", k => k));

    [Fact]
    public void ShouldPrintToLocalView_WhenSelfOrTargetLocal() =>
        Assert.True(VmActionPrintLogRecoil.ShouldPrintToLocalView(false, true));

    [Fact]
    public void ShouldSkipLocalLog_WhenLocalAndNotSelfView() =>
        Assert.True(VmActionPrintLogRecoil.ShouldSkipLocalLog(true, false));

    [Fact]
    public void SelectMidPrintDuration_UsesDefaultWhenNonPositive() =>
        Assert.Equal(3.0, VmActionPrintLogRecoil.SelectMidPrintDuration(0, 3.0));

    [Fact]
    public void MapTranslucentMode_MapsFuzzyMode() =>
        Assert.Equal(VmActionPrintLogRecoil.StyleFuzzy, VmActionPrintLogRecoil.MapTranslucentMode(2));

    [Fact]
    public void ClampAlpha_BoundsToUnitInterval() =>
        Assert.Equal(1.0, VmActionPrintLogRecoil.ClampAlpha(2.0));

    [Fact]
    public void WithTemporaryMidTime_SwapsDuration()
    {
        var (saved, temp) = VmActionPrintLogRecoil.WithTemporaryMidTime(2, 5, 3);
        Assert.Equal(5, saved);
        Assert.Equal(2, temp);
    }

    [Fact]
    public void PrintBoldAlwaysShows() =>
        Assert.True(VmActionPrintLogRecoil.PrintBoldAlwaysShows());

    [Fact]
    public void FormatLogInt_MatchesPrintf() =>
        Assert.Equal("42", VmActionPrintLogRecoil.FormatLogInt(42));

    [Fact]
    public void FormatLogFloat_MatchesPrintf() =>
        Assert.Equal("1.5", VmActionPrintLogRecoil.FormatLogFloat(1.5));

    [Fact]
    public void ApplySetRenderStyle_ClampsAlpha()
    {
        var (a, s) = VmActionPrintLogRecoil.ApplySetRenderStyle(2, 4);
        Assert.Equal(1.0, a);
        Assert.Equal(4, s);
    }

    [Fact]
    public void ShouldClearAlpha1Flag_IsTrue() =>
        Assert.True(VmActionPrintLogRecoil.ShouldClearAlpha1Flag());

    [Fact]
    public void FadeReduceDefault_UsesTenthWhenZero() =>
        Assert.Equal(0.1, VmActionPrintLogRecoil.FadeReduceDefault(0));

    [Fact]
    public void ClearAlpha1RenderStyleFlag_ClearsBit() =>
        Assert.Equal(0, VmActionPrintLogRecoil.ClearAlpha1RenderStyleFlag(VmActionPrintLogRecoil.StyleAlpha1));

    [Fact]
    public void AddFadeInReduce_UsesDefaultReduce() =>
        Assert.Equal(0.6, VmActionPrintLogRecoil.AddFadeInReduce(0.5, 0));

    [Fact]
    public void LogLocalGatePasses_WhenNotSkipped() =>
        Assert.True(VmActionPrintLogRecoil.LogLocalGatePasses(false, true));

    [Fact]
    public void StepFadeIn_ClampsAndMarksRemove()
    {
        var r = VmActionPrintLogRecoil.StepFadeIn(0.95, 0.1, VmActionPrintLogRecoil.FtfClamp | VmActionPrintLogRecoil.FtfRemove);
        Assert.True(r.clampToOne);
        Assert.True(r.shouldRemove);
        Assert.Equal(1.0, r.newAlpha);
    }
}
