namespace HCDE.Client.Tests;

public class VmAmMapDisplayInit1130SliceTests
{
    [Fact]
    public void FindInitialPlayer_SkipsConsoleWhenAbsentAndCentersCamera()
    {
        var inGame = new bool[VmAmMapDisplayInit1130Slice.MaxPlayers];
        inGame[3] = true;
        Assert.Equal(3, VmAmMapDisplayInit1130Slice.FindInitialPlayer(0, inGame));
        Assert.Equal((84d, 40d), VmAmMapDisplayInit1130Slice.CenterOnCamera(100, 50, 32, 20));
        VmAmMapDisplayInit1130Slice.RememberWindow(84, 40, 32, 20, out var ox, out var oy, out var ow, out var oh);
        Assert.Equal((84d, 40d, 32d, 20d), (ox, oy, ow, oh));
    }

    [Fact]
    public void ClearMarks_EmptiesSlotsAndLevelScaleFallsBackToMinimum()
    {
        var marks = new double[10];
        Assert.True(VmAmMapDisplayInit1130Slice.ClearMarks(marks, out var next, true));
        Assert.Equal(0, next);
        Assert.All(marks, x => Assert.Equal(-1, x));
        Assert.Equal("AUTOPAGE", VmAmMapDisplayInit1130Slice.BackgroundTextureName(""));
        Assert.Equal("MAP01", VmAmMapDisplayInit1130Slice.BackgroundTextureName("MAP01"));
        var (scale, inverse) = VmAmMapDisplayInit1130Slice.InitialLevelScale(0.7, 0.5);
        Assert.Equal(0.7, scale);
        Assert.Equal(1 / 0.7, inverse);
    }

    [Fact]
    public void LineThickness_RoundsHalfEvenAndResolutionKeepsRatio()
    {
        Assert.Equal(1, VmAmMapDisplayInit1130Slice.LineThickness(false, 4, 4));
        Assert.Equal(2, VmAmMapDisplayInit1130Slice.LineThickness(true, 1.5, 4));
        Assert.Equal(2, VmAmMapDisplayInit1130Slice.LineThickness(true, 2.5, 2.5));
        Assert.Equal(1, VmAmMapDisplayInit1130Slice.LineThickness(true, 0.4, 0.4));
        Assert.True(VmAmMapDisplayInit1130Slice.TryNewResolution(2, 4, 3, 10, out var scale, out var inverse));
        Assert.Equal(6, scale);
        Assert.Equal(1 / 6d, inverse);
        Assert.False(VmAmMapDisplayInit1130Slice.TryNewResolution(0, 4, 3, 10, out var kept, out _));
        Assert.Equal(4, kept);
    }

    [Fact]
    public void SuppressesFollowPan_OnlyForAutomapPanBinds()
    {
        Assert.True(VmAmMapDisplayInit1130Slice.SuppressesFollowPan(true, true, true, "+AM_PANLEFT"));
        Assert.False(VmAmMapDisplayInit1130Slice.SuppressesFollowPan(true, true, false, "+am_panleft"));
        Assert.False(VmAmMapDisplayInit1130Slice.SuppressesFollowPan(true, true, true, "+am_zoom"));
        var (minScale, inverse) = VmAmMapDisplayInit1130Slice.MinOut(0.25);
        Assert.Equal(0.25, minScale);
        Assert.Equal(4, inverse);
    }
}
