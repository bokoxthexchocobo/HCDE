namespace HCDE.Client.Tests;

public class VmAmMapScaleMark1128SliceTests
{
    [Fact]
    public void GetPosition_IsWindowCenter()
    {
        var window = new VmAmMapScaleMark1128Slice.MapWindow { X = 10, Y = 20, W = 8, H = 4 };
        Assert.Equal((14d, 22d), VmAmMapScaleMark1128Slice.GetPosition(window));
    }

    [Fact]
    public void ActivateNewScale_RecentersOnFrameToMapSize()
    {
        var window = new VmAmMapScaleMark1128Slice.MapWindow
        {
            X = 0, Y = 0, W = 100, H = 80, FrameW = 320, FrameH = 200, ScaleFrameToMap = 0.5
        };
        VmAmMapScaleMark1128Slice.ActivateNewScale(window);
        Assert.Equal(-30, window.X);
        Assert.Equal(-10, window.Y);
        Assert.Equal(160, window.W);
        Assert.Equal(100, window.H);
        Assert.Equal(130, window.X2);
        Assert.Equal(90, window.Y2);
    }

    [Fact]
    public void RestoreScaleAndLoc_FollowsCameraOrSavedOrigin()
    {
        var parked = new VmAmMapScaleMark1128Slice.MapWindow
        {
            X = 4, Y = 6, W = 100, H = 40, FrameW = 320, FrameH = 200
        };
        VmAmMapScaleMark1128Slice.SaveScaleAndLoc(parked);
        parked.X = 0;
        parked.Y = 0;
        parked.W = 1;
        VmAmMapScaleMark1128Slice.RestoreScaleAndLoc(parked, false, 200, 80);
        Assert.Equal(4, parked.X);
        Assert.Equal(6, parked.Y);
        Assert.Equal(3.2, parked.ScaleMapToFrame);
        Assert.Equal(1 / 3.2, parked.ScaleFrameToMap);

        var follow = new VmAmMapScaleMark1128Slice.MapWindow
        {
            OldW = 100, OldH = 40, FrameW = 320, FrameH = 200
        };
        VmAmMapScaleMark1128Slice.RestoreScaleAndLoc(follow, true, 200, 80);
        Assert.Equal(150, follow.X);
        Assert.Equal(60, follow.Y);
    }

    [Fact]
    public void TryAddMark_WrapsAndRejectsMissingDefaultDigits()
    {
        var window = new VmAmMapScaleMark1128Slice.MapWindow
        {
            X = 0, Y = 0, W = 10, H = 6, MarkIndex = 9
        };
        Assert.Equal(-1, VmAmMapScaleMark1128Slice.TryAddMark(window, "ammnumx", false));
        Assert.Equal(9, window.MarkIndex);
        Assert.Equal(9, VmAmMapScaleMark1128Slice.TryAddMark(window, "AMMNUMx", true));
        Assert.Equal(0, window.MarkIndex);
        Assert.Equal((5d, 3d), window.Marks[9]);
    }

    [Fact]
    public void SeedVertexBounds_UsesFloatMaxAndFixedMin()
    {
        var (minX, minY, maxX, maxY) = VmAmMapScaleMark1128Slice.SeedVertexBounds();
        Assert.Equal((double)float.MaxValue, minX);
        Assert.Equal(minX, minY);
        Assert.Equal((double)int.MinValue, maxX);
        Assert.Equal(maxX, maxY);
        Assert.True(VmAmMapScaleMark1128Slice.VertexXIsNewMinimum(-1, minX));
        Assert.False(VmAmMapScaleMark1128Slice.VertexXIsNewMinimum(maxX, maxX));
    }
}
