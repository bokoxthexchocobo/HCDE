namespace HCDE.Client.Tests;

public class VmMusicWildMidiInstrumentsTests
{
    [Fact]
    public void SetupUsesDefaultClientReaderThenSkipsAlreadyLoadedNameIgnoringCase()
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var reader = new object();
        Assert.True(cache.SetupConfiguration("", "Bank.cfg", name =>
            { Assert.Equal("Bank.cfg", name); return reader; }, _ => throw new Exception(),
            _ => throw new Exception(), _ => throw new Exception(), _ => throw new Exception()));
        Assert.Same(reader, cache.PendingReader);
        cache.Load(44100, (_, _) => new());
        Assert.False(cache.SetupConfiguration("BANK.CFG", "unused", _ => throw new Exception(),
            _ => throw new Exception(), _ => throw new Exception(), _ => throw new Exception(), _ => throw new Exception()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetupZipFallsBackToFileSystemOnlyWhenZipIsRejected(bool validZip)
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var reader = new object();
        var calls = new List<string>();
        Assert.True(cache.SetupConfiguration("bank.zip", "unused", _ => null, _ => true,
            _ => "PK\u0003\u0004rest"u8.ToArray(), _ => { calls.Add("zip"); return validZip ? reader : null; },
            _ => { calls.Add("file"); return reader; }));
        Assert.Equal(validZip ? new[] { "zip" } : new[] { "zip", "file" }, calls);
        Assert.Same(reader, cache.PendingReader);
    }

    [Fact]
    public void SetupMissingConfigurationLeavesPendingReaderUnchanged()
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var reader = new object();
        cache.SetReader("old", reader);
        var error = Assert.Throws<InvalidOperationException>(() => cache.SetupConfiguration("missing", "unused",
            _ => null, _ => false, _ => throw new Exception(), _ => throw new Exception(), _ => throw new Exception()));
        Assert.Equal("WildMidi: missing: Unable to load sound font\n", error.Message);
        Assert.Same(reader, cache.PendingReader);
        Assert.Equal("old", cache.ReaderName);
    }

    private sealed class Instruments : IVmMusicWildMidiInstruments
    {
        public int Result;
        public int Loads;
        public int LoadConfiguration(string? name) { Assert.Null(name); Loads++; return Result; }
    }

    [Fact]
    public void PendingReaderIsTransferredOnceAndCachedInstrumentsAreReloaded()
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var reader = new object();
        var instruments = new Instruments();
        cache.SetReader("bank.cfg", reader);
        Assert.Same(instruments, cache.Load(44100, (input, rate) =>
        { Assert.Same(reader, input); Assert.Equal(44100, rate); return instruments; }));
        Assert.Null(cache.PendingReader);
        Assert.Equal("bank.cfg", cache.LoadedConfiguration);
        Assert.Same(instruments, cache.Load(48000, (_, _) => throw new Exception("unexpected allocation")));
        Assert.Equal(2, instruments.Loads);
    }

    [Fact]
    public void ConfigurationFailureClearsCacheButDoesNotInvalidateEarlierBorrowers()
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var instruments = new Instruments();
        cache.SetReader("first", new());
        var borrowed = cache.Load(44100, (_, _) => instruments);
        instruments.Result = -1;
        Assert.Throws<InvalidOperationException>(() => cache.Load(44100, (_, _) => throw new Exception()));
        Assert.Null(cache.Instruments);
        Assert.Equal("", cache.LoadedConfiguration);
        Assert.Same(instruments, borrowed);
        Assert.Throws<InvalidOperationException>(() => cache.Load(44100, (_, _) => new()));
        cache.SetReader("second", new());
        Assert.NotSame(borrowed, cache.Load(44100, (_, _) => new()));
    }

    [Fact]
    public void ConstructorFailureRetainsPendingReaderAndOriginalError()
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var reader = new object();
        var error = new IOException("allocation failed");
        cache.SetReader("bank", reader);
        Assert.Same(error, Assert.Throws<IOException>(() => cache.Load(44100, (_, _) => throw error)));
        Assert.Same(reader, cache.PendingReader);
        Assert.Null(cache.Instruments);
        Assert.Equal("bank", cache.LoadedConfiguration);
    }

    [Fact]
    public void ReplacementRetainsExistingInstrumentReferences()
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        cache.SetReader("old", new());
        var old = cache.Load(44100, (_, _) => new());
        cache.SetReader("new", new());
        var replacement = cache.Load(44100, (_, _) => new());
        Assert.NotSame(old, replacement);
        Assert.Same(replacement, cache.Instruments);
        Assert.Equal(1, old.Loads);
        Assert.Equal("new", cache.LoadedConfiguration);
    }
}
