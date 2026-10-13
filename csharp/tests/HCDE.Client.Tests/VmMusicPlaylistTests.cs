namespace HCDE.Client.Tests;

public class VmMusicPlaylistTests
{
    [Theory]
    [InlineData(false, "b")]
    [InlineData(true, "c")]
    public void ActivationSkipsFailedSongsInRequestedDirection(bool backwards, string playable)
    {
        var playlist = new VmMusicPlaylist(["a", "b", "c"]);
        var calls = new List<string>();
        Assert.True(playlist.Activate(backwards, force => { Assert.True(force); calls.Add("stop"); },
            (song, subsong, loop, force) =>
            {
                Assert.Equal(0, subsong);
                Assert.False(loop);
                Assert.True(force);
                calls.Add(song);
                return song == playable;
            }, _ => throw new InvalidOperationException()));
        Assert.Equal(new[] { "stop", "a", playable }, calls);
        Assert.Equal(playable, playlist.GetSong(playlist.Position));
        Assert.Equal(3, playlist.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExhaustionAttemptsEachSongOnceThenClearsAndReports(bool backwards)
    {
        var playlist = new VmMusicPlaylist(["a", "b", "c"]);
        playlist.SetPosition(1);
        var attempts = new List<string>();
        Assert.False(playlist.Activate(backwards, _ => { }, (song, _, _, _) => { attempts.Add(song); return false; },
            message => { Assert.Equal(0, playlist.Count); Assert.Equal("Cannot play anything in the playlist.", message); }));
        Assert.Equal(backwards ? new[] { "b", "a", "c" } : new[] { "b", "c", "a" }, attempts);
        Assert.Equal(0, playlist.Position);
    }

    [Fact]
    public void NavigationWrapsAndInvalidPositionsResetToZero()
    {
        var playlist = new VmMusicPlaylist(["a", "b"]);
        Assert.Equal(1, playlist.Backup());
        Assert.Equal(0, playlist.Advance());
        Assert.Equal(0, playlist.SetPosition(-1));
        Assert.Equal(0, playlist.SetPosition(2));
        Assert.Null(playlist.GetSong(-1));
        Assert.Null(playlist.GetSong(2));
        playlist.Clear();
        Assert.Equal(0, playlist.Backup());
        Assert.Throws<InvalidOperationException>(() => playlist.Activate(false, _ => { }, (_, _, _, _) => true, _ => { }));
    }
}
