namespace HCDE.Client.Tests;

public class VmMusicRestartTests
{
    [Fact]
    public void RestartClearsSavedNameBeforeForcedChangeAndForwardsOrderAndLoop()
    {
        var state = new VmMusicPlaybackState<object>("track", null, 3, true);
        Assert.Equal(("track", 3), state.GetMusic());
        state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { });
        Assert.Equal((null, 0), state.GetMusic());
        var calls = 0;
        state.Restart(1, true, (name, order, loop, force) =>
        {
            Assert.Equal("track", name);
            Assert.Equal(3, order);
            Assert.True(loop);
            Assert.True(force);
            Assert.Equal("", state.LastSong);
            calls++;
            return false;
        }, _ => throw new InvalidOperationException());
        Assert.Equal(1, calls);
        Assert.Equal("", state.LastSong);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NonpositiveVolumeSuppressesRestartAndPreservesSavedSong(float volume)
    {
        var state = new VmMusicPlaybackState<object>("track", null);
        state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { });
        state.Restart(volume, true, (_, _, _, _) => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException());
        Assert.Equal("track", state.LastSong);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void MissingRestartSongOrDisabledMusicForceStops(bool enabled, bool saveSong)
    {
        var state = new VmMusicPlaybackState<object>("track", null);
        if (saveSong) state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { });
        var stops = 0;
        state.Restart(1, enabled, (_, _, _, _) => throw new InvalidOperationException(),
            force => { Assert.True(force); stops++; });
        Assert.Equal(1, stops);
    }
}
