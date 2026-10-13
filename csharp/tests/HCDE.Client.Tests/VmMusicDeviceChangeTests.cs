namespace HCDE.Client.Tests;

public class VmMusicDeviceChangeTests
{
    [Fact]
    public void PlayingMidiReloadUsesMetadataSnapshotAfterForcedStop()
    {
        var song = new object();
        var state = new VmMusicPlaybackState<object>("track", song, 4, true);
        var calls = new List<string>();
        state.MidiDeviceChanged(handle => { Assert.Same(song, handle); calls.Add("midi"); return true; },
            handle => { Assert.Same(song, handle); calls.Add("playing"); return true; }, force =>
            {
                Assert.True(force);
                calls.Add("stop");
                state.Stop(force, 0, () => { }, () => { }, _ => { }, _ => { });
            }, (name, order, loop) =>
            {
                Assert.Equal("", state.Name);
                Assert.Equal("track", name);
                Assert.Equal(4, order);
                Assert.True(loop);
                calls.Add("change");
                return false;
            });
        Assert.Equal(new[] { "midi", "playing", "stop", "change" }, calls);
    }

    [Theory]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    public void MissingNonMidiAndNotPlayingSongsDoNotReload(bool hasSong, bool midi, bool playing)
    {
        var state = new VmMusicPlaybackState<object>("track", hasSong ? new object() : null);
        var queries = new List<string>();
        state.MidiDeviceChanged(_ => { queries.Add("midi"); return midi; },
            _ => { queries.Add("playing"); return playing; }, _ => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException());
        Assert.Equal(!hasSong ? Array.Empty<string>() : !midi ? new[] { "midi" } : new[] { "midi", "playing" }, queries);
    }

    [Fact]
    public void StopFailurePropagatesWithoutReload()
    {
        var state = new VmMusicPlaybackState<object>("track", new object());
        Assert.Throws<InvalidOperationException>(() => state.MidiDeviceChanged(_ => true, _ => true,
            _ => throw new InvalidOperationException("stop failed"), (_, _, _) => throw new Exception("unexpected reload")));
    }
}
