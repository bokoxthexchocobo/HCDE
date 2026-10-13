using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiInstrumentCatalogTests
{
    private static byte[] Bytes()
    {
        var bytes = new byte[337];
        "GF1PATCH110\0ID#000002\0"u8.CopyTo(bytes);
        bytes[198] = 1; bytes[247] = 2; bytes[259] = 1; bytes[269] = 1;
        return bytes;
    }

    [Fact]
    public void RendererLoadsDefaultResourceAndReleasesOwnedPatch()
    {
        var reads = new List<string>();
        var catalog = new VmMusicWildMidiInstrumentCatalog(name => { reads.Add(name); return Bytes(); });
        var patch = catalog.Register(0, "piano.pat");
        using (var renderer = new VmMusicWildMidiManagedRenderer(44100, catalog.FindExact, catalog.LoadSamples))
        {
            Assert.Equal(new[] { "piano.pat" }, reads);
            Assert.True(patch.Loaded);
            Assert.Equal(1, patch.UseCount);
            renderer.LoadInstrument(0, 0, 0);
            Assert.Single(reads);
            renderer.ShortEvent(0x90, 60, 100);
            Assert.Equal(1, renderer.GetVoiceCount());
        }
        Assert.False(patch.Loaded);
        Assert.Equal(0, patch.UseCount);
    }

    [Fact]
    public void MissingOrForeignResourcesFailWithoutMarkingPatchLoaded()
    {
        var catalog = new VmMusicWildMidiInstrumentCatalog(_ => null);
        var patch = catalog.Register(1, "missing.pat");
        Assert.Throws<FileNotFoundException>(() => catalog.LoadSamples(patch));
        Assert.False(patch.Loaded);
        Assert.Throws<ArgumentException>(() => catalog.LoadSamples(new(1)));
        Assert.Null(catalog.FindExact(2));
    }
}
