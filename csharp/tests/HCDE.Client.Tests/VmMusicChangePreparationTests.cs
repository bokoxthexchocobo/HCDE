namespace HCDE.Client.Tests;

public class VmMusicChangePreparationTests
{
    [Theory]
    [InlineData(false, 0, VmMusicChangeDisposition.Disabled)]
    [InlineData(true, 1, VmMusicChangeDisposition.Handled)]
    public void EntryGuardsSkipLookupAndStop(bool enabled, int playlistCount, VmMusicChangeDisposition expected)
    {
        var state = new VmMusicPlaybackState<object>("track", new());
        var result = state.PrepareChange(enabled, playlistCount, false, new("next", 2),
            _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
        Assert.Equal(expected, result.Disposition);
        Assert.Equal("track", state.Name);
    }

    [Fact]
    public void ForcedChangeResolvesNameAndMutableOrderBeforeContinuing()
    {
        var state = new VmMusicPlaybackState<object>("track", new());
        var result = state.PrepareChange(true, 1, true, new("alias", 0),
            _ => throw new InvalidOperationException(), request =>
            {
                Assert.Equal("alias", request.Name);
                return new("resolved", 4);
            });
        Assert.Equal(VmMusicChangeDisposition.Continue, result.Disposition);
        Assert.Equal(new VmMusicChangeRequest("resolved", 4), result.Request);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EmptyResolvedSongForceStopsThenClearsRestartName(string? name)
    {
        var state = new VmMusicPlaybackState<object>("track", null);
        var result = state.PrepareChange(true, 0, false, new(name, 0), force =>
        {
            Assert.True(force);
            state.Stop(force, 0, () => { }, () => { }, _ => { }, _ => { });
            Assert.Equal("track", state.LastSong);
        });
        Assert.Equal(VmMusicChangeDisposition.Handled, result.Disposition);
        Assert.Equal("", state.Name);
        Assert.Equal("", state.LastSong);
    }
}
