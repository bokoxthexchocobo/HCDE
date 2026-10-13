using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiSourceFactoryTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResourceSourceFactoryDiscoversArchivePatchesAndReleasesArchiveOnFailure(bool missingPatch)
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bank");
        try
        {
            using (var file = File.Create(path))
            using (var zip = new System.IO.Compression.ZipArchive(file, System.IO.Compression.ZipArchiveMode.Create))
            {
                using (var config = new StreamWriter(zip.CreateEntry("timidity.cfg").Open()))
                    config.Write("0 piano\n1 violin");
                using (var patch = zip.CreateEntry("piano.pat").Open()) patch.Write(Bytes());
                if (!missingPatch)
                {
                    using var patch = zip.CreateEntry("violin.pat").Open();
                    patch.Write(Bytes());
                }
            }
            using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000,
                [new(0, 0x01c0), new(0, 0x643c90)]));
            if (missingPatch)
                Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFactory.CreateResourceForSource(path, source, 44100, false, false));
            else
            {
                using var renderer = VmMusicWildMidiFactory.CreateResourceForSource(path, source, 44100, false, false);
                Assert.Throws<IOException>(() => File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None));
                renderer.ShortEvent(0xc0, 1, 0);
                renderer.ShortEvent(0x90, 60, 100);
                var output = new float[400];
                renderer.ComputeOutput(output, 200);
                Assert.Contains(output, value => value != 0f);
            }
            Assert.False(source.CheckDone());
            using var exclusive = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        finally { File.Delete(path); }
    }

    private static byte[] Bytes()
    {
        var bytes = new byte[399];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(247), 64);
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(259), 44100);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(269), 262000);
        bytes.AsSpan(335).Fill(64);
        return bytes;
    }

    [Fact]
    public void SourceDiscoveryLoadsBankedAndDrumPatchesAndRewindsSource()
    {
        VmMusicSmfEvent[] events = [new(0, 0x0200b0), new(0, 0x01c0), new(0, 0x643c90), new(0, 0x03c9), new(0, 0x642399)];
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, events));
        var reads = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { reads.Add(name); return Bytes(); });
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano\n1 violin\nbank 2\n1 banked\ndrumset 0\n35 kick\ndrumset 3\n35 banked-kick");
        using (var renderer = VmMusicWildMidiFactory.CreateForSource(catalog, source, 44100, false, true))
        {
            Assert.Equal(new[] { "piano.pat", "violin.pat", "banked.pat", "kick.pat", "banked-kick.pat" }, reads);
            Assert.False(source.CheckDone());
            Assert.Equal(1, source.LoopLimit);
            foreach (var item in events)
                renderer.ShortEvent((int)(item.Event & 255), (int)((item.Event >> 8) & 127), (int)((item.Event >> 16) & 127));
            Assert.Equal(2, renderer.GetVoiceCount());
            var output = new float[20];
            renderer.ComputeOutput(output, 10);
            Assert.Contains(output, value => value != 0f);
        }
        Assert.False(source.CheckDone());
        Assert.False(catalog.FindExact(0x201)!.Loaded);
    }

    [Fact]
    public void FailedDiscoveredPatchLoadReleasesDefaultAndLeavesSourceRewound()
    {
        using var source = new VmMusicSmfPlaybackSource(VmMusicSmf.Serialize(480, 500000, [new(0, 0x01c0)]));
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => name == "piano.pat" ? Bytes() : null);
        catalog.Register(0, "piano.pat");
        catalog.Register(1, "missing.pat");
        Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFactory.CreateForSource(catalog, source, 44100, false, false));
        Assert.False(catalog.FindExact(0)!.Loaded);
        Assert.Equal(0, catalog.FindExact(0)!.UseCount);
        Assert.False(source.CheckDone());
    }
}
