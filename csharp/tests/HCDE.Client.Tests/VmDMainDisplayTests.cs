namespace HCDE.Client.Tests;

public class VmDMainDisplayTests
{
    [Fact]
    public void ShouldSkipDisplay_WhenDedicated()
    {
        Assert.True(VmDMainDisplay.ShouldSkipDisplay(false, true, true));
    }

    [Fact]
    public void ShouldEnterLevelRenderBranch_RequiresGametic()
    {
        Assert.False(VmDMainDisplay.ShouldEnterLevelRenderBranch(1, 1, 2, 0));
    }

    [Fact]
    public void ShouldBlockInactiveRender_WhenUserGameInactive()
    {
        Assert.True(VmDMainDisplay.ShouldBlockInactiveRender(
            false, false, false, true, 2, 0, 1, 3, false));
    }

    [Fact]
    public void ResolveWipeType_ForceFadeSelectsFade()
    {
        Assert.Equal(7, VmDMainDisplay.ResolveWipeType(99, 1, 99, 2, 3, 7, 8, 9));
    }

    [Fact]
    public void ShouldStartWipe_BlocksVrPaletteMode()
    {
        Assert.False(VmDMainDisplay.ShouldStartWipe(1, 0, 2, 3, 1, 4));
    }
}

public class VmDMainDrawRateOverlayTests
{
    [Fact]
    public void ClampTicDelta_CapsAtTwenty()
    {
        Assert.Equal(20, VmDMainDrawRateOverlay.ClampTicDelta(99));
    }
}

public class VmDMainRendererCapsTests
{
    [Fact]
    public void BuildHardwareCaps_AddsVoxelsWhenEnabled()
    {
        var caps = VmDMainRendererCaps.BuildHardwareCaps(false, true);
        Assert.NotEqual(0u, caps & VmDMainRendererCaps.RffVoxels);
    }
}

public class VmDMainPaletteTesterTests
{
    [Fact]
    public void ResolveCellCoordinates_MapsLinearIndex()
    {
        var (row, col) = VmDMainPaletteTester.ResolveCellCoordinates(17);
        Assert.Equal(1, row);
        Assert.Equal(1, col);
    }
}

public class VmDMainLagHudOverlayTests
{
    [Fact]
    public void ResolveLagHudColor_StallIsRed()
    {
        Assert.Equal(VmDMainLagHudOverlay.CrRed, VmDMainLagHudOverlay.ResolveLagHudColor(true, 0, 35, 2));
    }
}
