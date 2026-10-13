using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigReplacementTests
{
    private static byte[] Bytes()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        return bytes;
    }

    [Fact]
    public void LastDeclarationWinsWithinItsBankAndResetsNoteOverride()
    {
        var reads = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { reads.Add(name); return Bytes(); });
        var paths = new VmMusicWildMidiConfigPaths("root/config.cfg", true);
        var original = catalog.RegisterConfigured(0, "first", paths, 60);
        var reader = new VmMusicWildMidiConfigReader(catalog, "root/config.cfg", true);
        reader.Read("bank 1\n0 banked\nbank 0\ndir updated\n0 last");
        Assert.Same(original, catalog.FindExact(0));
        Assert.Equal(0, original.Instrument.Note);
        catalog.LoadSamples(original);
        catalog.LoadSamples(catalog.FindExact(0x100)!);
        Assert.Equal(new[] { "updated/last.pat", "root/banked.pat" }, reads);
    }

    [Fact]
    public void LoadedReplacementPreservesSamplesAndReferencesUntilReleased()
    {
        var reads = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { reads.Add(name); return Bytes(); });
        var paths = new VmMusicWildMidiConfigPaths("config.cfg", true);
        var patch = catalog.RegisterConfigured(0, "first", paths, 60);
        catalog.LoadSamples(patch);
        var samples = patch.Instrument.Samples;
        patch.UseCount = 2;
        Assert.Same(patch, catalog.RegisterConfigured(0, "second", paths));
        Assert.True(patch.Loaded);
        Assert.Equal(2, patch.UseCount);
        Assert.Same(samples, patch.Instrument.Samples);
        Assert.Equal(0, patch.Instrument.Note);
        catalog.LoadSamples(patch);
        Assert.Equal(new[] { "first.pat" }, reads);
        patch.UseCount = 0;
        patch.FreeSamples();
        catalog.LoadSamples(patch);
        Assert.Equal(new[] { "first.pat", "second.pat" }, reads);
    }

    [Fact]
    public void InvalidReplacementPreservesOriginalConfiguration()
    {
        string? read = null;
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { read = name; return Bytes(); });
        var paths = new VmMusicWildMidiConfigPaths("config.cfg", true);
        var patch = catalog.RegisterConfigured(1, "first", paths, 60);
        Assert.Throws<ArgumentOutOfRangeException>(() => catalog.RegisterConfigured(1, "second", paths, 128));
        Assert.Throws<ArgumentException>(() => catalog.RegisterConfigured(1, "", paths));
        Assert.Throws<ArgumentException>(() => catalog.Register(1, "duplicate.pat"));
        Assert.Equal(60, patch.Instrument.Note);
        catalog.LoadSamples(patch);
        Assert.Equal("first.pat", read);
    }
}
