namespace HCDE.Client.Tests;

public class VmMusicStartOpenedTests
{
    [Fact]
    public void SuccessfulStartSetsMetadataBeforeCallbacksAndOrderAfterStreamCreation()
    {
        var state = new VmMusicPlaybackState<object>("old", null, 5);
        state.Stop(true, 0, () => { }, () => { }, _ => { }, _ => { });
        var song = new object();
        Assert.True(state.StartOpened("new", song, 3, true, (handle, loop, volume, order) =>
        {
            Assert.Same(song, handle);
            Assert.Same(song, state.Song);
            Assert.Equal("new", state.Name);
            Assert.Equal("", state.LastSong);
            Assert.Equal(0, state.BaseOrder);
            Assert.True(loop);
            Assert.Equal(1, volume);
            Assert.Equal(3, order);
            return true;
        }, () => throw new InvalidOperationException(), () => Assert.Equal(0, state.BaseOrder)));
        Assert.Equal(3, state.BaseOrder);
        Assert.True(state.Loop);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MissingOrFailedStartLeavesNativeNameAndZeroOrderWithoutCreatingStream(bool hasSong)
    {
        var state = new VmMusicPlaybackState<object>("", null);
        var reports = 0;
        Assert.False(state.StartOpened("new", hasSong ? new object() : null, 3, false,
            (_, _, _, _) => false, () => reports++, () => throw new InvalidOperationException()));
        Assert.Equal(hasSong ? 1 : 0, reports);
        Assert.Equal("new", state.Name);
        Assert.Equal(0, state.BaseOrder);
        Assert.Equal(hasSong, state.Song is not null);
    }

    [Fact]
    public void AdoptingBeforeClosingOldHandleReportsAnErrorWithoutOverwritingIt()
    {
        var song = new object();
        var state = new VmMusicPlaybackState<object>("old", song);
        Assert.Throws<InvalidOperationException>(() => state.StartOpened("new", new(), 0, false,
            (_, _, _, _) => true, () => { }, () => { }));
        Assert.Same(song, state.Song);
        Assert.Equal("old", state.Name);
    }
}
