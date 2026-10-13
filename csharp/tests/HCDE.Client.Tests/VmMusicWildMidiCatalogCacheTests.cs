using HCDE.Client;
using Xunit;

namespace HCDE.Client.Tests;

public sealed class VmMusicWildMidiCatalogCacheTests
{
    [Fact]
    public void ReusesNamesCaseInsensitivelyAndPreservesPreviousEntryOnFailure()
    {
        var cache = new VmMusicWildMidiCatalogCache();
        var first = new VmMusicWildMidiInstrumentCatalog(_ => null);
        Assert.Same(first, cache.Load("bank.cfg", _ => first));
        Assert.Same(first, cache.Load("BANK.CFG", _ => throw new InvalidOperationException("unexpected reload")));
        Assert.Throws<FileNotFoundException>(() => cache.Load("missing.cfg", _ => throw new FileNotFoundException()));
        Assert.Equal("bank.cfg", cache.LoadedConfiguration);
        Assert.Same(first, cache.Load("bank.cfg", _ => throw new InvalidOperationException("unexpected reload")));
        cache.Clear();
        var second = new VmMusicWildMidiInstrumentCatalog(_ => null);
        Assert.Same(second, cache.Load("bank.cfg", _ => second));
    }

    [Fact]
    public void CachedFileFactoryReusesParsedConfigurationUntilExplicitClear()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".cfg");
        var cache = new VmMusicWildMidiCatalogCache();
        try
        {
            File.WriteAllText(path, "");
            var factory = VmMusicWildMidiFactory.CreateCachedFileWaveFactory(cache, path, false, false);
            using (var renderer = factory(new(6, null, "unused.wav", 0, 22050))) Assert.Equal(22050, renderer.SampleRate);
            File.WriteAllText(path, "source missing.cfg");
            using (var renderer = factory(new(6, null, "unused.wav", 0, 0))) Assert.Equal(44100, renderer.SampleRate);
            cache.Clear();
            Assert.Throws<FileNotFoundException>(() => factory(new(6, null, "unused.wav", 0, 0)));
        }
        finally { File.Delete(path); }
    }
}
