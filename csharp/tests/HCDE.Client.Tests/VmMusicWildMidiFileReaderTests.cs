using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiFileReaderTests
{
    [Fact]
    public void ReadsExactBytesAndClosesFile()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, [0, 1, 255, 0]);
            Assert.Equal(new byte[] { 0, 1, 255, 0 }, VmMusicWildMidiFileReader.Read(path));
            using var exclusive = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            Assert.Equal(4, exclusive.Length);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void MissingFileFailsExplicitly()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".pat");
        Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFileReader.Read(path));
    }

    [Fact]
    public void FailedChannelInitializationReleasesAcquiredDefaultPatch()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        var patch = new VmMusicWildMidiInstrumentPatch(0);
        patch.Load(bytes);
        var calls = 0;
        var error = new InvalidOperationException("lookup failed");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => new VmMusicWildMidiManagedRenderer(44100,
            _ => ++calls == 1 ? patch : throw error, _ => 0)));
        Assert.Equal(0, patch.UseCount);
        Assert.False(patch.Loaded);
        Assert.False(patch.HasSamples);
    }
}
