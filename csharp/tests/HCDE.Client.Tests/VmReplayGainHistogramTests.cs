namespace HCDE.Client.Tests;

public class VmReplayGainHistogramTests
{
    [Fact]
    public void EmptyHistogramsReturnNativeSentinel()
    {
        var histogram = new VmReplayGainHistogram();
        Assert.Equal(-24601, histogram.GetTitleGain());
        Assert.Equal(-24601, histogram.GetAlbumGain());
    }

    [Fact]
    public void FinalizingTitleMovesWindowsToAlbumAndClearsTitle()
    {
        var histogram = new VmReplayGainHistogram();
        histogram.AddWindow(100, 100, 1);
        var expected = 64.82f - 20;
        Assert.Equal(expected, histogram.GetTitleGain());
        Assert.Equal(expected, histogram.GetAlbumGain());
        Assert.Equal(-24601, histogram.GetTitleGain());
        Assert.Equal(expected, histogram.GetAlbumGain());
    }

    [Fact]
    public void PercentileSelectsLoudTailRatherThanAverage()
    {
        var histogram = new VmReplayGainHistogram();
        for (var i = 0; i < 18; i++) histogram.AddWindow(1, 1, 1);
        for (var i = 0; i < 2; i++) histogram.AddWindow(100, 100, 1);
        Assert.Equal(64.82f - 20, histogram.GetTitleGain());
    }

    [Fact]
    public void SilentAndOversizedWindowsClampToHistogramEndpoints()
    {
        var histogram = new VmReplayGainHistogram();
        histogram.AddWindow(0, 0, 1);
        Assert.Equal(64.82f, histogram.GetTitleGain());
        histogram.AddWindow(1e20, 1e20, 1);
        Assert.Equal(64.82f - 119.99f, histogram.GetTitleGain());
    }
}
