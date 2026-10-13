namespace HCDE.Client.Tests;

public class VmMusicDeferredTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(-1, true)]
    [InlineData(1, false)]
    public void MutedOrDisabledMusicRecordsMetadataForLaterRestart(float volume, bool enabled)
    {
        var state = new VmMusicPlaybackState<object>("", null);
        Assert.True(state.RecordDeferred("track", 4, true, volume, enabled));
        Assert.Equal(("track", 4), state.GetMusic());
        Assert.Equal("track", state.LastSong);
        Assert.True(state.Loop);
        Assert.Null(state.Song);
        state.Restart(1, true, (name, order, loop, force) =>
        {
            Assert.Equal("track", name);
            Assert.Equal(4, order);
            Assert.True(loop);
            Assert.True(force);
            return true;
        }, _ => throw new InvalidOperationException());
    }

    [Fact]
    public void AudibleMusicContinuesWithoutChangingStateAndLiveHandleCannotBeOverwritten()
    {
        var state = new VmMusicPlaybackState<object>("old", new());
        Assert.False(state.RecordDeferred("new", 3, false, 1, true));
        Assert.Equal("old", state.Name);
        Assert.Throws<InvalidOperationException>(() => state.RecordDeferred("new", 3, false, 0, true));
        Assert.Equal("old", state.Name);
    }

    [Theory]
    [InlineData("file://music.mid", "music.mid")]
    [InlineData("FILE://music.mid", "FILE://music.mid")]
    [InlineData("file://file://music.mid", "file://music.mid")]
    [InlineData("https://host/song", "https://host/song")]
    public void ChangeNameStripsOnlyOneExactLowercaseFilePrefix(string input, string expected)
        => Assert.Equal(expected, VmMusicResources.ChangeName(input));
}
