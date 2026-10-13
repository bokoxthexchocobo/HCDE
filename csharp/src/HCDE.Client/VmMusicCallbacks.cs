namespace HCDE.Client;

public sealed class VmMusicCallbacks : IDisposable
{
    private readonly IReadOnlyList<VmMusicBank> banks;
    private readonly Func<string, int>? findResource;
    private readonly Func<int, byte[]?>? reopenResource;
    private readonly HashSet<VmMusicReaderHandle> handles = [];
    private bool disposed;

    public VmMusicCallbacks(IReadOnlyList<VmMusicBank> banks, Func<string, int>? findResource = null,
        Func<int, byte[]?>? reopenResource = null)
    {
        ArgumentNullException.ThrowIfNull(banks);
        if ((findResource is null) != (reopenResource is null))
            throw new ArgumentException("Resource callbacks must be supplied together");
        this.banks = Array.AsReadOnly(banks.ToArray());
        this.findResource = findResource;
        this.reopenResource = reopenResource;
    }

    public string? PathToBank(string? name, VmMusicBankFormat allowed) =>
        VmMusicBankCatalog.Find(banks, name, allowed)?.Path;

    public string ExpandPath(string? path) => VmSearchPathExpansion.ExpandEnvironment(path);

    public VmMusicReaderHandle? OpenBank(string? name, VmMusicBankFormat allowed)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var reader = VmMusicBankReaderFactory.Open(name, allowed, banks, findResource, reopenResource);
        if (reader is null) return null;
        var handle = new VmMusicReaderHandle(reader, closed => handles.Remove(closed));
        handles.Add(handle);
        return handle;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (var handle in handles.ToArray()) handle.Dispose();
    }
}

public sealed class VmMusicReaderHandle : IDisposable
{
    private readonly IVmMusicBankReader reader;
    private bool disposed;
    private readonly Action<VmMusicReaderHandle>? closed;

    internal VmMusicReaderHandle(IVmMusicBankReader reader, Action<VmMusicReaderHandle>? closed = null)
    { this.reader = reader; this.closed = closed; }

    public (byte[]? Data, string Name) OpenFile(string? name)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        return reader.Open(name);
    }

    public void AddPath(string path)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        reader.Paths.Add(path);
    }

    public VmMusicByteReader? OpenByteReader(string? name)
    {
        var result = OpenFile(name);
        return result.Data is { } data ? new VmMusicByteReader(data) : null;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { if (reader is IDisposable owned) owned.Dispose(); }
        finally { closed?.Invoke(this); }
    }
}
