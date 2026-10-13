namespace HCDE.Client.Tests;

public class VmMusicConsoleTests
{
    [Fact]
    public void StopCommandClearsPlaylistBeforeUnforcedStopAndForgetsRestart()
    {
        var state = new VmMusicPlaybackState<object>("old", null);
        var playlist = new VmMusicPlaylist(["listed"]);
        var console = new VmMusicConsole<object>(state, playlist, () => false,
            (_, _) => throw new InvalidOperationException(), force =>
            {
                Assert.False(force);
                Assert.Equal(0, playlist.Count);
                state.Stop(force, playlist.Count, () => { }, () => { }, _ => { }, _ => { });
                Assert.Equal("old", state.LastSong);
            }, _ => { }, _ => { });
        Assert.True(console.TryExecute(["STOPMUS"]));
        Assert.Equal("", state.LastSong);
        Assert.Equal("", state.Name);
    }

    [Fact]
    public void ChangeUsesLiveEnablementAndReportsCurrentState()
    {
        var enabled = false;
        var messages = new List<string>();
        var changes = new List<(string, int)>();
        var console = new VmMusicConsole<object>(new("old", null), new(["listed"]), () => enabled,
            (name, order) => { changes.Add((name, order)); return false; }, _ => { }, _ => { }, messages.Add);
        Assert.True(console.TryExecute(["changemus", "new", "2tail"]));
        Assert.Empty(changes);
        enabled = true;
        Assert.True(console.TryExecute(["changemus", "new", "2tail"]));
        Assert.Equal(new[] { ("new", 2) }, changes);
        Assert.True(console.TryExecute(["currentmusic"]));
        Assert.Equal(new[] { "Music is disabled\n", "Currently playing music 'old'\n" }, messages);
    }

    [Fact]
    public void PlaylistCommandsRoutePositionDirectionAndStatus()
    {
        var playlist = new VmMusicPlaylist(["first", "second"]);
        var directions = new List<bool>();
        var messages = new List<string>();
        var console = new VmMusicConsole<object>(new("", null), playlist, () => true,
            (_, _) => false, _ => { }, directions.Add, messages.Add);
        Assert.True(console.TryExecute(["playlistpos", "2"]));
        Assert.Equal(1, playlist.Position);
        Assert.True(console.TryExecute(["playlistnext"]));
        Assert.Equal(0, playlist.Position);
        Assert.True(console.TryExecute(["playlistprev"]));
        Assert.Equal(1, playlist.Position);
        Assert.True(console.TryExecute(["playliststatus"]));
        Assert.Equal(new[] { false, false, true }, directions);
        Assert.Equal(new[] { "Song 2 of 2:\nsecond\n" }, messages);
    }

    [Fact]
    public void EmptyUnknownAndMissingPositionPreservePlaylist()
    {
        var playlist = new VmMusicPlaylist(["first"]);
        var console = new VmMusicConsole<object>(new("", null), playlist, () => throw new InvalidOperationException(),
            (_, _) => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
        Assert.False(console.TryExecute([]));
        Assert.False(console.TryExecute(["unknown"]));
        Assert.True(console.TryExecute(["playlistpos"]));
        Assert.Equal(1, playlist.Count);
    }
}
