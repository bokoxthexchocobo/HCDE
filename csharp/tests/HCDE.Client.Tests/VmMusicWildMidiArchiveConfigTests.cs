using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiArchiveConfigTests
{
    [Fact]
    public void ArchiveIncludesAndCaseInsensitivePatchLookupFeedManagedRenderer()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        var patch = new byte[399];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(patch);
        patch[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(patch.AsSpan(247), 64);
        BinaryPrimitives.WriteUInt16LittleEndian(patch.AsSpan(259), 44100);
        BinaryPrimitives.WriteUInt32LittleEndian(patch.AsSpan(269), 262000);
        patch.AsSpan(335).Fill(64);
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Write(zip, "timidity.cfg", Encoding.ASCII.GetBytes("source banks/tones.cfg"));
                Write(zip, "banks/tones.cfg", Encoding.ASCII.GetBytes("0 Piano"));
                Write(zip, "banks/piano.pat", patch);
            }
            using (var reader = new VmGusZipMusicReader(path))
            {
                var catalog = VmMusicWildMidiConfigLoader.LoadBank(reader, false);
                Assert.False(catalog.FindExact(0)!.Loaded);
                using (var renderer = VmMusicWildMidiFactory.CreateConfigured(catalog, 44100, false, false))
                {
                    renderer.ShortEvent(0x90, 60, 100);
                    var output = new float[400];
                    renderer.ComputeOutput(output, 200);
                    Assert.Contains(output, value => value != 0);
                }
                Assert.False(catalog.FindExact(0)!.Loaded);
            }
            using (var renderer = VmMusicWildMidiFactory.CreateFromArchive(path, 44100, false, false))
            {
                renderer.ShortEvent(0x90, 60, 100);
                var output = new float[400];
                renderer.ComputeOutput(output, 200);
                Assert.Contains(output, value => value != 0);
            }
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void MissingDefaultConfigurationFailsExplicitly()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create)) Write(zip, "other.cfg", []);
            using var reader = new VmGusZipMusicReader(path);
            Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiConfigLoader.LoadBank(reader, false));
            Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFactory.CreateFromArchive(path, 44100, false, false));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void StrictArchiveReaderDoesNotUseExistingDiskResource()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        var disk = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pat");
        try
        {
            File.WriteAllBytes(disk, [42]);
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create)) Write(zip, "timidity.cfg", []);
            using var reader = new VmGusZipMusicReader(path, allowDiskFallback: false);
            Assert.NotNull(reader.Open(null).Data);
            Assert.Null(reader.Open(disk).Data);
        }
        finally { File.Delete(path); File.Delete(disk); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ArchiveWaveFactoryOwnsReaderAndClosesArchiveAfterExport(bool detectArchive)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bank");
        var wave = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create)) Write(zip, "timidity.cfg", []);
            using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
                [new(0, 0x643c90), new(120, 0x3c80)]));
            var factory = detectArchive ? VmMusicWildMidiFactory.CreateWaveFactory(path, false, false)
                : VmMusicWildMidiFactory.CreateArchiveWaveFactory(path, false, false);
            Assert.True(VmMusicWildMidiWaveDump.Write(source, new(6, null, wave, 0, 0), factory));
            Assert.True(new FileInfo(wave).Length > 68);
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); File.Delete(wave); }
    }

    [Theory]
    [InlineData(3, true)]
    [InlineData(4, false)]
    public void ArchiveEntrySizeLimitIsCheckedBeforeExtraction(int limit, bool exceedsLimit)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create)) Write(zip, "timidity.cfg", [1, 2, 3, 4]);
            using var reader = new VmGusZipMusicReader(path, false, limit);
            if (exceedsLimit) Assert.Throws<InvalidDataException>(() => reader.Open(null));
            else Assert.Equal(new byte[] { 1, 2, 3, 4 }, reader.Open(null).Data);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 2)]
    public void DuplicateEntryPrecedenceMatchesSelectedReaderPolicy(bool allowDiskFallback, byte expected)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Write(zip, "timidity.cfg", [1]);
                Write(zip, "TIMIDITY.CFG", [2]);
            }
            using var reader = new VmGusZipMusicReader(path, allowDiskFallback);
            Assert.Equal(new[] { expected }, reader.Open(null).Data);
            Assert.Equal(new[] { expected }, reader.Open("TiMiDiTy.CfG").Data);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void OwnedArchiveFactoryUsesFirstConfigurationEntry()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".zip");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZipArchive(file, ZipArchiveMode.Create))
            {
                Write(zip, "timidity.cfg", []);
                Write(zip, "TIMIDITY.CFG", Encoding.ASCII.GetBytes("source missing.cfg"));
            }
            using (var renderer = VmMusicWildMidiFactory.CreateFromArchive(path, 44100, false, false))
                Assert.Equal(0, renderer.GetVoiceCount());
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); }
    }

    private static void Write(ZipArchive archive, string name, byte[] bytes)
    {
        using var output = archive.CreateEntry(name).Open();
        output.Write(bytes);
    }
}
