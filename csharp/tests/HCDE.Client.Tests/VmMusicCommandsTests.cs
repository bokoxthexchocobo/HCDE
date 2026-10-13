namespace HCDE.Client.Tests;

public class VmMusicCommandsTests
{
    [Theory]
    [InlineData(false, "old", "Music is disabled\n")]
    [InlineData(true, "old", "currently playing old\n")]
    [InlineData(true, "", "no music playing\n")]
    public void DisabledOrStatusRequestsReportWithoutChangingMusic(bool enabled, string current, string expected)
    {
        var messages = new List<string>();
        VmMusicCommands.Change(new VmMusicPlaybackState<object>(current, null), new VmMusicPlaylist([]),
            enabled, null, 0, (_, _) => throw new InvalidOperationException(), messages.Add);
        Assert.Equal(new[] { expected }, messages);
    }

    [Fact]
    public void ExplicitSongClearsPlaylistBeforeChangingAndStartUsesZeroNonLooping()
    {
        var playlist = new VmMusicPlaylist(["listed"]);
        VmMusicCommands.Change(new VmMusicPlaybackState<object>("", null), playlist, true, "", 3,
            (name, order) => { Assert.Equal(0, playlist.Count); Assert.Equal("", name); Assert.Equal(3, order); return false; },
            _ => throw new InvalidOperationException());
        Assert.False(VmMusicCommands.Start("song", (name, order, loop) =>
        { Assert.Equal("song", name); Assert.Equal(0, order); Assert.False(loop); return false; }));
    }
}
