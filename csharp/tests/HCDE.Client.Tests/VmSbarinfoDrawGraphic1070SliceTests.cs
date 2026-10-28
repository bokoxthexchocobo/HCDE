namespace HCDE.Client.Tests;

public class VmSbarinfoDrawGraphic1070SliceTests
{
    [Fact]
    public void UsesCenterOffset_DetectsNativeFlag()
    {
        Assert.True(VmSbarinfoDrawGraphic1070Slice.UsesCenterOffset(VmSbarinfoDrawGraphic1070Slice.OffsetCenter));
    }

    [Fact]
    public void ApplyCenteredDrawX_UsesTextureWidthWhenForcedUnset()
    {
        var dx = VmSbarinfoDrawGraphic1070Slice.ApplyCenteredDrawX(100, 40, 5, -1);
        Assert.Equal(85, dx);
    }

    [Fact]
    public void ApplyPixelOffsets_AddsBlockOffsets()
    {
        var (x, y) = VmSbarinfoDrawGraphic1070Slice.ApplyPixelOffsets(10, 20, 3, -2);
        Assert.Equal(13, x);
        Assert.Equal(18, y);
    }

    [Fact]
    public void ResolveDestSize_HonorsForcedDimensions()
    {
        var (w, h) = VmSbarinfoDrawGraphic1070Slice.ResolveDestSize(64, 32, 16, 8);
        Assert.Equal(64, w);
        Assert.Equal(32, h);
    }

    [Fact]
    public void ComputeStatusbarClipLeft_ZeroWhenClipDisabled()
    {
        Assert.Equal(0, VmSbarinfoDrawGraphic1070Slice.ComputeStatusbarClipLeft(10, 4, 2, clipDisabled: true));
    }

    [Fact]
    public void ResolveColorOverlay_MatchesDimOverlayConstant()
    {
        Assert.Equal(VmSbarinfoDrawGraphic1070Slice.DimOverlayArgb,
            VmSbarinfoDrawGraphic1070Slice.ResolveColorOverlay(dim: true));
    }

    [Fact]
    public void ApplyFullscreenRelativeCenter_DelegatesTo1068Adjust()
    {
        var (rx, ry) = VmSbarinfoDrawGraphic1070Slice.ApplyFullscreenRelativeCenter(
            true, false, 5, 7, 200, 100, 1.0, 1.0);
        Assert.Equal(105, rx);
        Assert.Equal(7, ry);
    }

    [Fact]
    public void ResolveFullscreenAnchor_FlipsNegativeNonRelativeX()
    {
        Assert.Equal(220, VmSbarinfoDrawGraphic1070Slice.ResolveFullscreenAnchor(-1, 20, true, false, 200));
    }

    [Fact]
    public void ScaleFullscreenDrawMetrics_AppliesHudScale()
    {
        var (w, h, rx, ry) = VmSbarinfoDrawGraphic1070Slice.ScaleFullscreenDrawMetrics(10, 20, 100, 50, 2, 0.5);
        Assert.Equal(200, w);
        Assert.Equal(25, h);
        Assert.Equal(20, rx);
        Assert.Equal(10, ry);
    }

    [Fact]
    public void HasAnyFullscreenClipComponent_DetectsPartialClip()
    {
        Assert.True(VmSbarinfoDrawGraphic1070Slice.HasAnyFullscreenClipComponent(
            clipLeftUnset: true, clipTopUnset: true, clipRightUnset: false, clipBottomUnset: true));
    }
}
