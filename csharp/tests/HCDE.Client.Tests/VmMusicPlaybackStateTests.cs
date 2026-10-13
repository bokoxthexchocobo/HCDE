namespace HCDE.Client.Tests;

public class VmMusicPlaybackStateTests
{
    [Fact]
    public void StopResumesThenStopsStreamAndSongAndDetachesBeforeClose()
    {
        var song = new object();
        var state = new VmMusicPlaybackState<object>("track", song);
        var calls = new List<string>();
        state.Stop(true, 2, () => calls.Add("resume"), () => calls.Add("stream"),
            handle => { Assert.Same(song, handle); Assert.Same(song, state.Song); calls.Add("stop"); },
            handle => { Assert.Same(song, handle); Assert.Null(state.Song); calls.Add("close"); });
        Assert.Equal(new[] { "resume", "stream", "stop", "close" }, calls);
        Assert.Equal("track", state.LastSong);
        Assert.Equal("", state.Name);
        Assert.Equal("Currently no music playing\n", state.CurrentMusicStatus());
    }

    [Fact]
    public void ActivePlaylistProtectsMusicUnlessForcedAndRepeatedStopDoesNothing()
    {
        var state = new VmMusicPlaybackState<object>("track", new object());
        Action fail = () => throw new InvalidOperationException();
        state.Stop(false, 1, fail, fail, _ => fail(), _ => fail());
        Assert.Equal("Currently playing music 'track'\n", state.CurrentMusicStatus());
        state.Stop(true, 1, () => { }, () => { }, _ => { }, _ => { });
        state.Stop(true, 0, fail, fail, _ => fail(), _ => fail());
        Assert.Equal("track", state.LastSong);
    }

    [Fact]
    public void HandlelessStopSavesNameAndStopCommandClearsPlaylistBeforeStoppingAndForgetsRestart()
    {
        var state = new VmMusicPlaybackState<object>("track", null);
        var playlist = new VmMusicPlaylist(["track"]);
        state.StopCommand(playlist, () =>
        {
            Assert.Equal(0, playlist.Count);
            state.Stop(false, playlist.Count, () => { }, () => { }, _ => { }, _ => { });
            Assert.Equal("track", state.LastSong);
        });
        Assert.Equal("", state.LastSong);
        Assert.Equal("", state.Name);
    }

    [Fact]
    public void CloseFailurePropagatesWithHandleAlreadyDetached()
    {
        var state = new VmMusicPlaybackState<object>("track", new object());
        Assert.Throws<InvalidOperationException>(() => state.Stop(true, 0, () => { }, () => { }, _ => { },
            _ => throw new InvalidOperationException("close failed")));
        Assert.Null(state.Song);
        Assert.Equal("track", state.Name);
        Assert.Equal("", state.LastSong);
    }
}
