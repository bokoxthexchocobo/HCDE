namespace HCDE.Client.Tests;

public class VmAutomapCustomCvarCallbacksTests
{
    [Fact]
    public void ClampAutomapCheat_BlocksNetWithoutCheats()
    {
        Assert.Equal(0, VmAutomapCustomCvarCallbacks.ClampAutomapCheat(true, false, 1));
    }

    [Fact]
    public void ShouldDeactivateOverlay_WhenOverlayZero()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.ShouldDeactivateOverlay(true, true, 0));
    }

    [Fact]
    public void ClampEmptySpaceMargin_CapsAtNinety()
    {
        Assert.Equal(90, VmAutomapCustomCvarCallbacks.ClampEmptySpaceMargin(120));
    }

    [Fact]
    public void DefaultShowSecrets_IsTrue()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.DefaultShowSecrets);
    }

    [Fact]
    public void ShouldUpdateShowAllLines_WhenAutomapPresent()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.ShouldUpdateShowAllLines(true));
    }

    [Fact]
    public void ShouldRefreshResolution_WhenStatusBarAndAutomap()
    {
        Assert.True(VmAutomapCustomCvarCallbacks.ShouldRefreshResolution(true, true));
    }

    [Fact]
    public void DefaultOverlaySectorFillAlpha_MatchesNative()
    {
        Assert.Equal(0.2f, VmAutomapCustomCvarCallbacks.DefaultOverlaySectorFillAlpha);
    }

    [Fact]
    public void ShouldInitColorFromCvar_SkipsAlmostBackground()
    {
        Assert.False(VmAutomapCustomCvarCallbacks.ShouldInitColorFromCvar(
            VmAutomapCustomCvarCallbacks.AlmostBackgroundColorIndex,
            VmAutomapCustomCvarCallbacks.AlmostBackgroundColorIndex));
    }

    [Fact]
    public void StandardCvarTableLength_MatchesNative()
    {
        Assert.Equal(25, VmAutomapCustomCvarCallbacks.StandardCvarTableLength);
    }

    [Fact]
    public void OverlayCvarTableLength_MatchesNative()
    {
        Assert.Equal(25, VmAutomapCustomCvarCallbacks.OverlayCvarTableLength);
    }

    [Fact]
    public void ApplySetWhite_FirstSlotBlackRestWhite()
    {
        Span<uint> colors = stackalloc uint[4];
        VmAutomapCustomCvarCallbacks.ApplySetWhite(colors);
        Assert.Equal(VmAutomapCustomCvarCallbacks.PackRgb(0, 0, 0), colors[0]);
        Assert.Equal(VmAutomapCustomCvarCallbacks.PackRgb(255, 255, 255), colors[1]);
    }

    [Fact]
    public void DecodePaletteTriplets_MarksUnusedTripletInvalid()
    {
        Span<uint> colors = stackalloc uint[2];
        Span<bool> valid = stackalloc bool[2];
        VmAutomapCustomCvarCallbacks.DecodePaletteTriplets(
            new byte[] { 1, 0, 0, 10, 20, 30 },
            colors,
            valid,
            true,
            out var fillAlpha);
        Assert.False(valid[0]);
        Assert.True(valid[1]);
        Assert.Equal(0f, fillAlpha);
    }

    [Fact]
    public void OverlayColorDefaults_LengthMatchesTable()
    {
        Assert.Equal(22, VmAutomapCustomCvarCallbacks.OverlayColorDefaults.Length);
    }
}
