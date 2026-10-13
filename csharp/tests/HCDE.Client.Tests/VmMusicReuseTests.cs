namespace HCDE.Client.Tests;

public class VmMusicReuseTests
{
    [Theory]
    [InlineData(true, 3)]
    [InlineData(false, 1)]
    public void SubsongChangeUpdatesOrderOnlyOnSuccess(bool succeeds, int expected)
    {
        var state = new VmMusicPlaybackState<object>("track", new(), 1);
        Assert.True(state.TryReuse("TRACK", 3, false, _ => false, (_, order) => { Assert.Equal(3, order); return succeeds; },
            _ => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException()));
        Assert.Equal(expected, state.BaseOrder);
    }

    [Fact]
    public void FailedRestartReportsThenCreatesStreamAndStillHandlesChange()
    {
        var state = new VmMusicPlaybackState<object>("track", new(), 2, true);
        var calls = new List<string>();
        Assert.True(state.TryReuse("track", 2, true, _ => true, (_, _) => throw new InvalidOperationException(),
            _ => false, (_, order, loop) => { Assert.Equal(2, order); Assert.True(loop); calls.Add("start"); return false; },
            () => calls.Add("error"), () => calls.Add("stream")));
        Assert.Equal(new[] { "start", "error", "stream" }, calls);
    }

    [Fact]
    public void NameOrLoopMismatchLeavesExistingSongUntouched()
    {
        var state = new VmMusicPlaybackState<object>("track", new());
        Assert.False(state.TryReuse("other", 0, false, _ => throw new InvalidOperationException(),
            (_, _) => false, _ => false, (_, _, _) => false, () => { }, () => { }));
        Assert.False(state.TryReuse("track", 0, true, _ => false,
            (_, _) => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), () => { }, () => { }));
    }
}
