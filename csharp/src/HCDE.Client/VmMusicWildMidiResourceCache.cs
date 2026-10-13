namespace HCDE.Client;

// The host serializes cache access and renderer disposal.
public sealed class VmMusicWildMidiResourceCache : IDisposable
{
    private sealed class Entry(VmMusicWildMidiInstrumentCatalog catalog, IDisposable? reader)
    {
        internal readonly VmMusicWildMidiInstrumentCatalog Catalog = catalog;
        private int owners = 1;

        internal IDisposable Retain()
        {
            owners = checked(owners + 1);
            return new Lease(this);
        }

        internal void Release()
        {
            if (--owners == 0) reader?.Dispose();
        }

        private sealed class Lease(Entry entry) : IDisposable
        {
            private Entry? owner = entry;
            public void Dispose()
            {
                var previous = owner;
                owner = null;
                previous?.Release();
            }
        }
    }

    private Entry? current;
    private readonly Func<string, IVmMusicBankReader?>? openBank;
    private bool disposed;
    public string LoadedConfiguration { get; private set; } = "";

    // A returned reader transfers ownership to the cache; null allows disk fallback.
    public VmMusicWildMidiResourceCache(Func<string, IVmMusicBankReader?>? openBank = null)
    {
        this.openBank = openBank;
    }

    public VmMusicWildMidiManagedRenderer CreateRendererForSource(string fileName, IVmMusicSmfSource source,
        int requestedSampleRate, bool enhancedResampling, bool reverb)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        ArgumentNullException.ThrowIfNull(source);
        var instruments = VmMusicMidiPrecache.Collect(source);
        return CreateRenderer(fileName, requestedSampleRate, enhancedResampling, reverb, instruments);
    }

    public VmMusicWildMidiManagedRenderer CreateRenderer(string fileName, int requestedSampleRate,
        bool enhancedResampling, bool reverb, IReadOnlyList<ushort>? instruments = null)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrEmpty(fileName);
        if (current is null || !LoadedConfiguration.Equals(fileName, StringComparison.OrdinalIgnoreCase))
        {
            var next = Load(fileName);
            var previous = current;
            current = next;
            LoadedConfiguration = fileName;
            previous?.Release();
        }

        var lease = current.Retain();
        try
        {
            var renderer = VmMusicWildMidiFactory.CreateConfigured(current.Catalog, requestedSampleRate,
                enhancedResampling, reverb, instruments);
            renderer.OwnBankReader(lease);
            return renderer;
        }
        catch (Exception initializationError)
        {
            try { lease.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("WildMIDI cached initialization and cleanup failed.", initializationError, cleanupError);
            }
            throw;
        }
    }

    private Entry Load(string fileName)
    {
        var supplied = openBank?.Invoke(fileName);
        if (supplied is not null) return LoadReader(supplied);
        if (!VmMusicWildMidiFactory.IsArchive(fileName))
            return new(VmMusicWildMidiConfigLoader.LoadFile(fileName), null);
        return LoadReader(new VmGusZipMusicReader(fileName, allowDiskFallback: false));
    }

    private static Entry LoadReader(IVmMusicBankReader reader)
    {
        var owned = reader as IDisposable;
        try
        {
            return new(VmMusicWildMidiConfigLoader.LoadBank(reader, OperatingSystem.IsWindows()), owned);
        }
        catch (Exception loadError)
        {
            try { owned?.Dispose(); }
            catch (Exception cleanupError)
            {
                throw new AggregateException("WildMIDI cached bank loading and cleanup failed.", loadError, cleanupError);
            }
            throw;
        }
    }

    public void Clear()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ReleaseCurrent();
    }

    private void ReleaseCurrent()
    {
        var previous = current;
        current = null;
        LoadedConfiguration = "";
        previous?.Release();
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        ReleaseCurrent();
    }
}
