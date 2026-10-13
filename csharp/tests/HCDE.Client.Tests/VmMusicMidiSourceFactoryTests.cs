namespace HCDE.Client.Tests;

public class VmMusicMidiSourceFactoryTests
{
    private static byte[] Mus()
    {
        var bytes = new byte[32];
        "MUS\u001a"u8.CopyTo(bytes);
        bytes[4] = 3;
        bytes[6] = 16;
        bytes[8] = 1;
        bytes[16] = 0x10; bytes[17] = 60; bytes[18] = 0x60;
        return bytes;
    }

    [Fact]
    public void FactoryCreatesOwnedMusSnapshotAndClearsPreviousError()
    {
        var factory = new VmMusicMidiSourceFactory();
        Assert.Null(factory.Create(ReadOnlyMemory<byte>.Empty, VmMusicMidiType.Mus));
        Assert.NotEmpty(factory.LastError);
        var data = Mus();
        using var source = factory.Create(data, VmMusicMidiType.Mus);
        Assert.IsType<VmMusicMusSource>(source);
        Assert.Equal("", factory.LastError);
        Array.Clear(data);
        Assert.Equal(VmMusicSmf.Serialize(140, 1000000, [new(0, 0x7f3c90)]), VmMusicSmfSource.Create(source!, 1));
    }

    [Theory]
    [InlineData(VmMusicMidiType.Hmi)]
    public void InvalidHmiReportsParserError(VmMusicMidiType type)
    {
        var factory = new VmMusicMidiSourceFactory();
        Assert.Null(factory.Create(ReadOnlyMemory<byte>.Empty, type));
        Assert.Equal("Truncated HMI/HMP header.", factory.LastError);
    }

    [Fact]
    public void UnknownFormatPreservesNativeIdentificationError()
    {
        var factory = new VmMusicMidiSourceFactory();
        Assert.Null(factory.Create(ReadOnlyMemory<byte>.Empty, VmMusicMidiType.None));
        Assert.Equal("Unable to identify MIDI data", factory.LastError);
    }

    [Fact]
    public void ResourceLookupCurrentSongAndFactoryExportProduceActualMidiFile()
    {
        var path = Path.Combine(Path.GetTempPath(), "hcde-resource-" + Guid.NewGuid().ToString("N") + ".mid");
        var calls = new List<string>();
        try
        {
            VmMusicMidiExport.WriteMidiFile(["writemidi", "*", path], name =>
                VmMusicMidiSource.OpenManaged(name, "D_TEST", resource =>
                    { Assert.Equal("D_TEST", resource); calls.Add("namespace"); return -1; },
                    resource => { Assert.Equal("D_TEST", resource); calls.Add("full"); return 3; },
                    index => { Assert.Equal(3, index); calls.Add("read"); return Mus(); },
                    _ => throw new Exception("unexpected report")), () => "unused", _ => throw new Exception("unexpected report"));
            Assert.Equal(new[] { "namespace", "full", "read" }, calls);
            Assert.Equal(VmMusicSmf.Serialize(140, 1000000, [new(0, 0x7f3c90)]), File.ReadAllBytes(path));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void ManagedLoaderReportsInvalidMusAndMidiHeaders()
    {
        var reports = new List<string>();
        var bytes = Mus(); bytes[8] = 16;
        Assert.Null(VmMusicMidiSource.OpenManaged("song", "", _ => 0, _ => -1, _ => bytes, reports.Add));
        Assert.Equal("Unable to open song: Invalid MUS channel count or score offset.\n", Assert.Single(reports));
        reports.Clear();
        "MThd"u8.CopyTo(bytes);
        Assert.Null(VmMusicMidiSource.OpenManaged("song", "", _ => 0, _ => -1, _ => bytes, reports.Add));
        Assert.Equal("Unable to open song: Invalid Standard MIDI File header.\n", Assert.Single(reports));
    }
}
