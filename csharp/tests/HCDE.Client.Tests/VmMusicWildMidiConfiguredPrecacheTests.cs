using System.Buffers.Binary;
using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfiguredPrecacheTests
{
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
    public void BankedAndDrumPrecacheLoadsOnceAndEnablesNotePlayback()
    {
        var reads = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { reads.Add(name); return Bytes(); });
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano\nbank 2\n1 violin\ndrumset 3\n35 kick");
        ushort[] instruments = [(2 << 7) | 1, (1 << 14) | (3 << 7) | 35, (2 << 7) | 1];
        using (var renderer = VmMusicWildMidiFactory.CreateConfigured(catalog, 44100, false, false, instruments))
        {
            Assert.Equal(new[] { "piano.pat", "violin.pat", "kick.pat" }, reads);
            renderer.ShortEvent(0xb0, 0, 2);
            renderer.ShortEvent(0xc0, 1, 0);
            renderer.ShortEvent(0x90, 60, 100);
            renderer.ShortEvent(0xc9, 3, 0);
            renderer.ShortEvent(0x99, 35, 100);
            Assert.Equal(2, renderer.GetVoiceCount());
            var output = new float[20];
            renderer.ComputeOutput(output, 10);
            Assert.Contains(output, value => value != 0f);
        }
        foreach (var id in new ushort[] { 0, 0x201, 0x3a3 })
        {
            Assert.False(catalog.FindExact(id)!.Loaded);
            Assert.Equal(0, catalog.FindExact(id)!.UseCount);
        }
    }

    [Fact]
    public void PrecacheFailureReleasesAlreadyAcquiredPatches()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => name == "missing.pat" ? null : Bytes());
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano\n1 violin\n2 missing");
        Assert.Throws<FileNotFoundException>(() => VmMusicWildMidiFactory.CreateConfigured(catalog, 44100, false, true, new ushort[] { 1, 2 }));
        foreach (var id in new ushort[] { 0, 1, 2 })
        {
            Assert.False(catalog.FindExact(id)!.Loaded);
            Assert.Equal(0, catalog.FindExact(id)!.UseCount);
        }
    }
}
