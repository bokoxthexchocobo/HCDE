using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiConfigReaderTests
{
    [Fact]
    public void ReadsMixedLineEndingsAndFinalLineIntoCatalog()
    {
        var reads = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { reads.Add(name); return null; });
        var reader = new VmMusicWildMidiConfigReader(catalog, "root/config.cfg", true);
        reader.Read("# header\r\n0 piano\rbank 2\n1 violin.PAT\ndir drums\r\ndrumset 3\n35 kick");
        foreach (var id in new ushort[] { 0, 0x201, 0x3a3 })
            Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(catalog.FindExact(id)!));
        Assert.Equal(new[] { "root/piano.pat", "root/violin.PAT", "drums/kick.pat" }, reads);
    }

    [Fact]
    public void ParsedDefaultPatchCanBeLoadedByRenderer()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => name == "piano.pat" ? bytes : null);
        new VmMusicWildMidiConfigReader(catalog, "config.cfg", true).Read("0 piano");
        using var renderer = new VmMusicWildMidiManagedRenderer(44100, catalog.FindExact, catalog.LoadSamples);
        renderer.ShortEvent(0x90, 60, 100);
        Assert.Equal(1, renderer.GetVoiceCount());
        Assert.True(catalog.FindExact(0)!.Loaded);
    }

    [Fact]
    public void UnsupportedOptionsAndMalformedDeclarationsDoNotRegisterPatch()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.ApplyLine("bank 1");
        Assert.Throws<InvalidDataException>(() => reader.ApplyLine("7"));
        Assert.Throws<NotSupportedException>(() => reader.ApplyLine("7 flute unconverted=80"));
        Assert.Null(catalog.FindExact(0x107));
        Assert.Throws<NotSupportedException>(() => reader.ApplyLine("source other.cfg"));
        reader.ApplyLine("7 flute");
        Assert.NotNull(catalog.FindExact(0x107));
    }

    [Fact]
    public void DuplicateDeclarationsPreserveIdentityAndNullTerminatesInput()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var reader = new VmMusicWildMidiConfigReader(catalog, "config.cfg", true);
        reader.Read("0 piano\0\n1 ignored");
        var original = catalog.FindExact(0);
        Assert.Null(catalog.FindExact(1));
        reader.ApplyLine("0 replacement");
        Assert.Same(original, catalog.FindExact(0));
    }
}
