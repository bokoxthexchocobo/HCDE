using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiWaveDumpTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExportsSourceAndReleasesOwnedRenderer(bool shortSong)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        var patchBytes = new byte[399];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(patchBytes);
        patchBytes[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(patchBytes.AsSpan(247), 64);
        BinaryPrimitives.WriteUInt16LittleEndian(patchBytes.AsSpan(259), 44100);
        BinaryPrimitives.WriteUInt32LittleEndian(patchBytes.AsSpan(269), 262000);
        patchBytes.AsSpan(335).Fill(64);
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => patchBytes);
        catalog.Register(0, "piano.pat");
        VmMusicWildMidiManagedRenderer? renderer = null;
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
            shortSong ? [new(0, 0x643c90), new(120, 0x3c80)]
                : [new(0, 0x643c90), new(480, 0x3c80), new(480, 0x643d90),
                    new(480, 0x3d80), new(480, 0x643e90), new(480, 0x3e80)]));
        try
        {
            Assert.True(VmMusicWildMidiWaveDump.Write(source, new(6, null, path, 0, 0),
                request => renderer = VmMusicWildMidiFactory.CreateConfigured(catalog, request.SampleRate, false, false)));
            var wave = File.ReadAllBytes(path);
            Assert.Equal("RIFF"u8.ToArray(), wave[..4]);
            Assert.Equal(44100u, BinaryPrimitives.ReadUInt32LittleEndian(wave.AsSpan(24)));
            Assert.True(wave.Length > 68);
            Assert.Contains(wave.AsSpan(68).ToArray(), value => value != 0);
            Assert.False(catalog.FindExact(0)!.Loaded);
            Assert.Throws<ObjectDisposedException>(() => renderer!.GetVoiceCount());
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void CancelledExportDoesNotAllocateRendererOrCreateFile()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, []));
        Assert.Throws<OperationCanceledException>(() => VmMusicWildMidiWaveDump.Write(source,
            new(6, null, path, 0, 44100), _ => throw new InvalidOperationException("unexpected allocation"),
            new CancellationToken(true)));
        Assert.False(File.Exists(path));
    }
}
