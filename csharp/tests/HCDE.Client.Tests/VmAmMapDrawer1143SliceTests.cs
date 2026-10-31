namespace HCDE.Client.Tests;

public class VmAmMapDrawer1143SliceTests
{
    [Fact]
    public void Markers_ScaleWithZoomOnlyWhenRequestedAndStopAfterASelfTarget()
    {
        Assert.True(VmAmMapDrawer1143Slice.TargetsSelf(0));
        Assert.False(VmAmMapDrawer1143Slice.AdvanceMarkedActors(0));
        Assert.True(VmAmMapDrawer1143Slice.AdvanceMarkedActors(4));
        Assert.Equal((2d, 6d), VmAmMapDrawer1143Slice.MarkerScale(2, 6, 0, 3));
        Assert.Equal((6d, 18d), VmAmMapDrawer1143Slice.MarkerScale(2, 6, 1, 3));
        Assert.True(VmAmMapDrawer1143Slice.DrawMarkedActor(0, 0));
        Assert.True(VmAmMapDrawer1143Slice.DrawMarkedActor(1, VmAmMapDrawer1143Slice.SubsectorDrawn));
        Assert.False(VmAmMapDrawer1143Slice.DrawMarkedActor(1, 0));
        Assert.False(VmAmMapDrawer1143Slice.DrawMarkedActor(2, VmAmMapDrawer1143Slice.SubsectorDrawn));
        Assert.Equal((5, 3), VmAmMapDrawer1143Slice.Crosshair(11, 5));
    }

    [Fact]
    public void ManualPan_KeepsAConstantPixelSpeedAndCancelsOppositeButtons()
    {
        Assert.Equal(1d, VmAmMapDrawer1143Slice.FrameDeltaSeconds(1500, 500));
        Assert.Equal(500ul, VmAmMapDrawer1143Slice.NextClock(false, 1500, 500));
        Assert.Equal(1500ul, VmAmMapDrawer1143Slice.NextClock(true, 1500, 500));
        var follow = VmAmMapDrawer1143Slice.ManualPan(true, true, false, false, false, 2, 0.5);
        Assert.False(follow.Applied);
        var left = VmAmMapDrawer1143Slice.ManualPan(false, true, false, false, false, 2, 0.5);
        Assert.Equal(-140d, left.X);
        Assert.Equal(0d, left.Y);
        var both = VmAmMapDrawer1143Slice.ManualPan(false, true, true, true, true, 2, 0.5);
        Assert.Equal(0d, both.X);
        Assert.Equal(0d, both.Y);
        Assert.True(VmAmMapDrawer1143Slice.ScaleChanges(false, false, -1));
        Assert.False(VmAmMapDrawer1143Slice.ScaleChanges(false, false, 0));
    }

    [Fact]
    public void Drawer_ClearsOnlyTheFullMapAndDrawsThingsForScannerOrCheat()
    {
        Assert.True(VmAmMapDrawer1143Slice.AllMap(VmAmMapDrawer1143Slice.LevelAllMap));
        Assert.False(VmAmMapDrawer1143Slice.AllThings(false, true));
        Assert.True(VmAmMapDrawer1143Slice.AllThings(true, true));
        Assert.Equal(0, VmAmMapDrawer1143Slice.PortalGroup(false, 4));
        Assert.Equal(4, VmAmMapDrawer1143Slice.PortalGroup(true, 4));
        var full = VmAmMapDrawer1143Slice.Frame(false, 320, 160, 8, 9, 100, 80);
        Assert.Equal(160, full.Height);
        Assert.True(full.Clear);
        var view = VmAmMapDrawer1143Slice.Frame(true, 320, 160, 8, 9, 100, 80);
        Assert.Equal(8, view.X);
        Assert.False(view.Clear);
        Assert.True(VmAmMapDrawer1143Slice.DrawSubsectors(true, false, false, 0));
        Assert.False(VmAmMapDrawer1143Slice.DrawSubsectors(true, true, true, 0));
        Assert.True(VmAmMapDrawer1143Slice.DrawSubsectors(false, true, true, 0.2));
        Assert.False(VmAmMapDrawer1143Slice.DrawKeys(false, false));
        Assert.True(VmAmMapDrawer1143Slice.DrawThings(2, false));
        Assert.False(VmAmMapDrawer1143Slice.DrawThings(4, false));
        Assert.True(VmAmMapDrawer1143Slice.DrawThings(4, true));
        Assert.True(VmAmMapDrawer1143Slice.DrawCrosshair(false));
        Assert.False(VmAmMapDrawer1143Slice.DrawCrosshair(true));
    }

    [Fact]
    public void SaveHead_StoresTwentyMarkDoublesAndBothScales()
    {
        var save = VmAmMapDrawer1143Slice.SaveHead(3, 0.2, 5);
        Assert.Equal(3, save.MarkIndex);
        Assert.Equal(20, save.MarkValueCount);
        Assert.Equal(0.2, save.ScaleMapToFrame);
        Assert.Equal(5d, save.ScaleFrameToMap);
    }
}
