namespace HCDE.Client.Tests;

public class VmMusicMidiExportTests
{
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
