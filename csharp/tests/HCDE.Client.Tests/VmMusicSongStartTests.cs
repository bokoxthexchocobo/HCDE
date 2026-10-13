namespace HCDE.Client.Tests;

public class VmMusicSongStartTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void StartAppliesRelativeLevelThenStopsAndUpdatesGainsAndOnlyNotifiesOnSuccess(bool success)
    {
        var volume = new VmMusicVolume();
        volume.SetRelativeLevel(.5f);
        var calls = new List<string>();
        Assert.Equal(success, VmMusicSongStart.Start(volume, .8f, false, 3, true,
            level => { Assert.Equal(.4f, level); calls.Add("relative"); },
            () => calls.Add("stop"), () => calls.Add("fluid"), () => calls.Add("module"),
            (subsong, loop) => { Assert.Equal(3, subsong); Assert.True(loop); calls.Add("start"); return success; },
            () => calls.Add("notify")));
        Assert.Equal(success ? new[] { "relative", "stop", "fluid", "module", "start", "notify" }
            : new[] { "relative", "stop", "fluid", "module", "start" }, calls);
        Assert.Equal(.8f, volume.SavedSongLevel);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(.8f, true)]
    public void SkippedRelativeUpdateStillRefreshesGainsAndStarts(float level, bool replayGain)
    {
        var volume = new VmMusicVolume();
        var gains = 0;
        Assert.True(VmMusicSongStart.Start(volume, level, replayGain, 0, false,
            _ => throw new InvalidOperationException(), () => { }, () => gains++, () => gains++,
            (_, _) => true, () => { }));
        Assert.Equal(2, gains);
        Assert.Equal(1, volume.SavedSongLevel);
    }

    [Fact]
    public void GainFailurePropagatesWithoutStartingSong()
    {
        Assert.Throws<InvalidOperationException>(() => VmMusicSongStart.Start(new VmMusicVolume(), 1, false, 0, false,
            _ => { }, () => { }, () => throw new InvalidOperationException("gain failed"),
            () => throw new Exception("unexpected module"), (_, _) => throw new Exception("unexpected start"),
            () => throw new Exception("unexpected notification")));
    }
}
