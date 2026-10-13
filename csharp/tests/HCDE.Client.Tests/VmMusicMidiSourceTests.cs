namespace HCDE.Client.Tests;

public class VmMusicMidiSourceTests
{
    [Fact]
    public void CurrentSongUsesNamespaceThenFullNameAndPassesAllBytesToFactory()
    {
        var bytes = new byte[40];
        "MThd"u8.CopyTo(bytes);
        var source = new object();
        var calls = new List<string>();
        Assert.Same(source, VmMusicMidiSource.Open("*", "current", name =>
            { Assert.Equal("current", name); calls.Add("namespace"); return -2; },
            name => { Assert.Equal("current", name); calls.Add("full"); return 3; },
            index => { Assert.Equal(3, index); return bytes; },
            (data, type) => { Assert.Equal(bytes, data.ToArray()); Assert.Equal(VmMusicMidiType.Midi, type); return source; },
            () => throw new InvalidOperationException(), _ => throw new InvalidOperationException()));
        Assert.Equal(new[] { "namespace", "full" }, calls);
    }

    [Theory]
    [InlineData(-1, -1, 40, "Cannot find MIDI lump song.\n")]
    [InlineData(0, -1, 31, "Unable to read lump song\n")]
    [InlineData(0, -1, 40, "song is not MIDI-based.\n")]
    public void MissingShortAndNonMidiResourcesReportWithoutCreatingSource(int primary, int fallback, int size, string expected)
    {
        var messages = new List<string>();
        Assert.Null(VmMusicMidiSource.Open<object>("song", "current", _ => primary,
            _ => { Assert.True(primary < 0); return fallback; }, _ => new byte[size],
            (_, _) => throw new InvalidOperationException(), () => throw new InvalidOperationException(), messages.Add));
        Assert.Equal(new[] { expected }, messages);
    }

    [Fact]
    public void DetectionIsLimitedToFirst32BytesAndFactoryFailureIncludesBackendError()
    {
        var bytes = new byte[40];
        "MUS\u001a"u8.CopyTo(bytes.AsSpan(32));
        var messages = new List<string>();
        object? Open() => VmMusicMidiSource.Open<object>("song", "", _ => 0,
            _ => throw new InvalidOperationException(), _ => bytes, (_, _) => null, () => "bad source", messages.Add);
        Assert.Null(Open());
        Assert.Equal("song is not MIDI-based.\n", Assert.Single(messages));
        messages.Clear();
        "MThd"u8.CopyTo(bytes);
        Assert.Null(Open());
        Assert.Equal("Unable to open song: bad source\n", Assert.Single(messages));
    }

    [Fact]
    public void StatisticsUsesHandlePresenceAndReturnsBackendTextUnchanged()
    {
        var song = new object();
        var state = new VmMusicPlaybackState<object>("", song);
        Assert.Equal("decoder stats", state.GetStatistics(handle => { Assert.Same(song, handle); return "decoder stats"; }));
        var deferred = new VmMusicPlaybackState<object>("song", null);
        Assert.Equal("No song playing", deferred.GetStatistics(_ => throw new InvalidOperationException()));
    }
}
