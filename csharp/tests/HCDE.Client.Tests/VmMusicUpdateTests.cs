namespace HCDE.Client.Tests;

public class VmMusicUpdateTests
{
    [Fact]
    public void MissingSongDoesNotCallTheBackendOrPlaylist()
    {
        Action unexpected = () => throw new InvalidOperationException("unexpected callback");
        VmMusicUpdate.Tick(false, unexpected, () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), unexpected, _ => unexpected(), _ => unexpected());
    }

    [Fact]
    public void PlayingSongUpdatesThenQueriesPlaybackOnEveryTick()
    {
        var calls = new List<string>();
        for (var i = 0; i < 2; i++)
            VmMusicUpdate.Tick(true, () => calls.Add("update"), () => { calls.Add("playing"); return true; },
                () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
                _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException());
        Assert.Equal(new[] { "update", "playing", "update", "playing" }, calls);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void FinishedSongAdvancesPlaylistOrForceStopsInNativeOrder(int count)
    {
        var calls = new List<string>();
        var currentCount = 0;
        VmMusicUpdate.Tick(true, () => { calls.Add("update"); currentCount = count; },
            () => { calls.Add("playing"); return false; },
            () => { calls.Add("count"); return currentCount; }, () => calls.Add("advance"),
            backwards => { Assert.False(backwards); calls.Add("activate"); },
            force => { Assert.True(force); calls.Add("stop"); });
        Assert.Equal(count == 0 ? new[] { "update", "playing", "count", "stop" }
            : new[] { "update", "playing", "count", "advance", "activate" }, calls);
    }

    [Fact]
    public void BackendFailurePropagatesAndDoesNotAdvancePlaylist()
    {
        var calls = new List<string>();
        Assert.Throws<InvalidOperationException>(() => VmMusicUpdate.Tick(true,
            () => calls.Add("update"), () => throw new InvalidOperationException("backend failed"),
            () => { calls.Add("count"); return 1; }, () => calls.Add("advance"),
            _ => calls.Add("activate"), _ => calls.Add("stop")));
        Assert.Equal(new[] { "update" }, calls);
    }
}
