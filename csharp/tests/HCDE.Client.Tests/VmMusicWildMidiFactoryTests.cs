namespace HCDE.Client.Tests;

public class VmMusicWildMidiFactoryTests
{
    private sealed class Instruments : IVmMusicWildMidiInstruments
    {
        public bool Loaded;
        public int LoadConfiguration(string? name) { Loaded = true; return 0; }
    }
    private sealed class Renderer : IVmMusicWildMidiRenderer, IDisposable
    {
        public Exception? InitializationError;
        public Exception? CleanupError;
        public int Disposals;
        public (int, int) Option;
        public void SetOption(int mask, int value) { Option = (mask, value); if (InitializationError is { } error) throw error; }
        public void Dispose() { Disposals++; if (CleanupError is { } error) throw error; }
        public void ShortEvent(int status, int first, int second) { }
        public void LongEvent(ReadOnlySpan<byte> data) { }
        public void ComputeOutput(Span<float> samples, int frames) { }
        public int GetVoiceCount() => 0;
        public void LoadInstrument(int bank, int drum, int instrument) { }
    }

    [Theory]
    [InlineData(11025, 11025)]
    [InlineData(65535, 65535)]
    [InlineData(11024, 44100)]
    [InlineData(65536, 44100)]
    public void CreationNormalizesRateLoadsInstrumentsThenInitializesRenderer(int requested, int expected)
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        var reader = new object();
        var instruments = new Instruments();
        var renderer = new Renderer();
        cache.SetReader("bank", reader);
        var actual = VmMusicWildMidiFactory.Create(cache, requested,
            (input, rate) => { Assert.Same(reader, input); Assert.Equal(expected, rate); return instruments; },
            input => { Assert.Same(instruments, input); Assert.True(input.Loaded); return renderer; }, true, false);
        Assert.Same(renderer, actual);
        Assert.Equal((6, 2), renderer.Option);
        Assert.Equal(0, renderer.Disposals);
        actual.Dispose();
        Assert.Equal(1, renderer.Disposals);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void InitializationFailureReleasesRendererAndPreservesCleanupFailure(bool cleanupFails)
    {
        var cache = new VmMusicWildMidiInstrumentCache<object, Instruments>();
        cache.SetReader("bank", new());
        var first = new IOException("initialize");
        var second = new IOException("cleanup");
        var renderer = new Renderer { InitializationError = first, CleanupError = cleanupFails ? second : null };
        var error = Record.Exception(() => VmMusicWildMidiFactory.Create(cache, 44100,
            (_, _) => new Instruments(), _ => renderer, false, true));
        if (cleanupFails) Assert.Equal(new Exception[] { first, second }, Assert.IsType<AggregateException>(error).InnerExceptions);
        else Assert.Same(first, error);
        Assert.Equal(1, renderer.Disposals);
        Assert.NotNull(cache.Instruments);
    }
}
