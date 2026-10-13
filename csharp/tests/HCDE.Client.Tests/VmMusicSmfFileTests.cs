namespace HCDE.Client.Tests;

public class VmMusicSmfFileTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), "hcde-smf-" + Guid.NewGuid().ToString("N"));

    public VmMusicSmfFileTests() => Directory.CreateDirectory(directory);
    public void Dispose() => Directory.Delete(directory, true);

    private sealed class Source : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void BinaryWriterPreservesNewlinesAndForwardsLoopLimit(int limit)
    {
        var path = Path.Combine(directory, "out.mid");
        File.WriteAllBytes(path, new byte[200]);
        var source = new Source();
        var expected = VmMusicSmf.Serialize(480, 500000,
            [new(10, 0x0a3c90), new(0, 0x80000003, new byte[] { 240, 10, 247 })]);
        Assert.True(VmMusicSmfFile.Write(source, path, limit, (handle, loops) =>
        { Assert.Same(source, handle); Assert.Equal(limit, loops); return expected; }));
        Assert.Equal(expected, File.ReadAllBytes(path));
        Assert.False(source.Disposed);
    }

    [Fact]
    public void MissingSourceDoesNotCreateFileOrGenerateData()
    {
        var path = Path.Combine(directory, "missing.mid");
        Assert.False(VmMusicSmfFile.Write<Source>(null, path, 1, (_, _) => throw new Exception("unexpected generation")));
        Assert.False(File.Exists(path));
    }

    [Fact]
    public void SerializerFailurePreservesExistingOutputAndPropagatesEvenIoExceptions()
    {
        var path = Path.Combine(directory, "out.mid");
        File.WriteAllBytes(path, [1, 2, 3]);
        Assert.Throws<IOException>(() => VmMusicSmfFile.Write(new Source(), path, 1,
            (_, _) => throw new IOException("source read failed")));
        Assert.Equal(new byte[] { 1, 2, 3 }, File.ReadAllBytes(path));
    }

    [Fact]
    public void ConsoleExportWritesConvertedMidiAndDisposesOwnedSource()
    {
        var sourcePath = Path.Combine(directory, "source.mid");
        var output = Path.Combine(directory, "out.mid");
        File.WriteAllBytes(sourcePath, [42]);
        var source = new Source();
        var expected = Convert.FromHexString("4D546864000000060000000101E04D54726B0000000B00FF510307A12000FF2F00");
        var console = new VmMusicConsole<object>(new("", null), new([]), () => false,
            (_, _) => false, _ => throw new Exception("unexpected stop"), _ => { }, _ => throw new Exception("unexpected report"),
            writeMidi: args => VmMusicMidiExport.WriteMidiFile(args, name =>
                { Assert.Equal(sourcePath, name); return source; }, (_, loops) =>
                { Assert.Equal(1, loops); return VmMusicSmf.Serialize(480, 500000, []); },
                () => "unused", _ => throw new Exception("unexpected report")));
        Assert.True(console.TryExecute(["writemidi", sourcePath, output]));
        Assert.Equal(expected, File.ReadAllBytes(output));
        Assert.Equal(new byte[] { 42 }, File.ReadAllBytes(sourcePath));
        Assert.True(source.Disposed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FileAccessFailureIsReportedByCommandAndDisposesSource(bool directoryTarget)
    {
        var output = directoryTarget ? directory : Path.Combine(directory, "absent", "out.mid");
        var source = new Source();
        var reports = new List<string>();
        VmMusicMidiExport.WriteMidiFile(["writemidi", "song", output], _ => source,
            (_, _) => VmMusicSmf.Serialize(480, 500000, []), () => "unused", reports.Add);
        Assert.Equal(new[] { $"Unable to write {output}\n" }, reports);
        Assert.True(source.Disposed);
        Assert.False(File.Exists(output));
    }
}
