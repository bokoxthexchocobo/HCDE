namespace HCDE.Client.Tests;

public class VmMusicMidiExportTests
{
    [Fact]
    public void RegisteredManagedExportResolvesCurrentSongAndWritesReadableMidiWithoutStoppingPlayback()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".mid");
        var song = new object();
        var state = new VmMusicPlaybackState<object>("current", song, 2, true);
        var bytes = VmMusicSmf.Serialize(120, 500000, [new VmMusicSmfEvent(1, 0x643c90)]);
        var lookups = new List<string>();
        var console = new VmMusicConsole<object>(state, new([]), () => true, (_, _) => false,
            _ => Assert.Fail("unexpected stop"), _ => { }, _ => Assert.Fail("unexpected report"));
        console.ConfigureMidiExport(name => { lookups.Add(name); return -1; },
            name => { Assert.Equal("current", name); return 3; },
            index => { Assert.Equal(3, index); return bytes; });
        try
        {
            Assert.True(console.TryExecute(["WRITEMIDI", "*", path]));
            using var exported = new VmMusicSmfPlaybackSource(File.ReadAllBytes(path));
            Assert.Equal(120, exported.Division);
            Assert.Equal(new[] { "current" }, lookups);
            Assert.Same(song, state.Song);
            Assert.Equal("current", state.Name);
            Assert.Equal(2, state.BaseOrder);
            Assert.True(state.Loop);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RegisteredManagedExportReportsUsageBeforeResourceLookup()
    {
        var reports = new List<string>();
        var console = new VmMusicConsole<object>(new("current", null), new([]), () => true,
            (_, _) => false, _ => Assert.Fail("unexpected stop"), _ => { }, reports.Add);
        console.ConfigureMidiExport(_ => throw new InvalidOperationException("unexpected lookup"),
            _ => throw new InvalidOperationException("unexpected fallback"),
            _ => throw new InvalidOperationException("unexpected read"));
        Assert.True(console.TryExecute(["writemidi"]));
        Assert.StartsWith("Usage: writemidi", Assert.Single(reports));
    }

    private sealed class Source : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DispatchUsesRequestedOutputAndDisposesSourceOnEitherResult(bool success)
    {
        var source = new Source();
        var messages = new List<string>();
        var console = new VmMusicConsole<object>(new("song", new()), new([]),
            () => throw new InvalidOperationException(), (_, _) => false, _ => throw new InvalidOperationException(),
            _ => { }, messages.Add, writeMidi: args => VmMusicMidiExport.WriteMidi(args,
                name => { Assert.Equal("*", name); return source; }, (handle, output, limit) =>
                { Assert.Same(source, handle); Assert.False(handle.Disposed); Assert.Equal("out.mid", output); Assert.Equal(1, limit); return success; },
                () => throw new InvalidOperationException(), messages.Add));
        Assert.True(console.TryExecute(["writemidi", "*", "out.mid"]));
        Assert.True(source.Disposed);
        Assert.Equal(success ? Array.Empty<string>() : new[] { "Unable to write out.mid\n" }, messages);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public void InvalidArgumentCountReportsUsageBeforeOpening(int count)
    {
        var messages = new List<string>();
        VmMusicMidiExport.WriteMidi<Source>(Enumerable.Repeat("arg", count).ToArray(),
            _ => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            () => throw new InvalidOperationException(), messages.Add);
        Assert.StartsWith("Usage: writemidi", Assert.Single(messages));
    }

    [Fact]
    public void MissingSourceReportsBackendErrorWithoutWriting()
    {
        var messages = new List<string>();
        VmMusicMidiExport.WriteMidi<Source>(["writemidi", "song", "out.mid"], _ => null,
            (_, _, _) => throw new InvalidOperationException(), () => "not found", messages.Add);
        Assert.Equal(new[] { "Unable to open song: not found\n" }, messages);
    }

    [Fact]
    public void ThrowingWriterStillDisposesSourceAndPropagatesError()
    {
        var source = new Source();
        Assert.Throws<InvalidOperationException>(() => VmMusicMidiExport.WriteMidi(
            ["writemidi", "song", "out.mid"], _ => source, (_, _, _) => throw new InvalidOperationException("write failed"),
            () => "unused", _ => throw new Exception("unexpected report")));
        Assert.True(source.Disposed);
    }
}
