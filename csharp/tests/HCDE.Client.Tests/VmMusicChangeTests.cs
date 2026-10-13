namespace HCDE.Client.Tests;

public class VmMusicChangeTests
{
    [Theory]
    [InlineData(false, 0, false, false)]
    [InlineData(true, 1, false, true)]
    public void EntryGuardsSkipAllBackendWork(bool enabled, int playlist, bool force, bool expected)
    {
        var state = new VmMusicPlaybackState<object>("old", new());
        Assert.Equal(expected, VmMusicChange.Change(state, new("new", 2), false, force, enabled, playlist,
            _ => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
            (_, _) => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException()));
        Assert.Equal("old", state.Name);
    }

    [Fact]
    public void LookupOrderAndNameFeedReuseAndFailedSubsongStillCountsAsHandled()
    {
        var state = new VmMusicPlaybackState<object>("track", new(), 1, true);
        Assert.True(VmMusicChange.Change(state, new("alias", 0), true, true, true, 1,
            _ => throw new InvalidOperationException(), _ => true,
            (_, order) => { Assert.Equal(3, order); return false; },
            _ => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), _ => new("TRACK", 3)));
        Assert.Equal(1, state.BaseOrder);
    }

    [Fact]
    public void FilePrefixDoesNotMatchReuseAndRoutesToConfiguredOpening()
    {
        var state = new VmMusicPlaybackState<object>("track", new(), 2, true);
        var resources = new VmMusicResources(_ => 0, _ => -1, _ => 1, _ => [42], _ => { });
        var calls = new List<string>();
        void Stop(bool force) => state.Stop(force, 0, () => { }, () => { }, _ => { }, _ => { });
        Assert.True(VmMusicChange.Change(state, new("file://track", 4), true, false, true, 0, Stop,
            _ => true, (_, _) => throw new InvalidOperationException(), _ => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), (name, order, loop) =>
                VmMusicChangeOpen.OpenAndStart(state, resources, name, order, loop, 1, true, Stop,
                    resolved => { Assert.Equal("track", resolved); return new(4, "bank", 7, 0); },
                    _ => calls.Add("volume"), (_, _, _) => throw new InvalidOperationException(),
                    _ => calls.Add("module"), (_, _, _) => { calls.Add("open"); return new object(); },
                    (_, _, _, _) => { calls.Add("start"); return true; },
                    _ => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
                    () => calls.Add("stream"))));
        Assert.Equal(new[] { "volume", "module", "open", "start", "stream" }, calls);
        Assert.Equal(("track", 4), state.GetMusic());
    }

    [Fact]
    public void EmptyLookupStopsAndClearsRestartWithoutOpening()
    {
        var state = new VmMusicPlaybackState<object>("old", null);
        Assert.True(VmMusicChange.Change(state, new("alias", 0), false, false, true, 0,
            force => state.Stop(force, 0, () => { }, () => { }, _ => { }, _ => { }),
            _ => throw new InvalidOperationException(), (_, _) => throw new InvalidOperationException(),
            _ => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), () => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), _ => new(null, 0)));
        Assert.Equal("", state.Name);
        Assert.Equal("", state.LastSong);
    }
}
