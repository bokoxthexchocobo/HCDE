using SharpCompress.Archives;

namespace HCDE.Client;

public sealed class VmGusZipMusicReader : IVmMusicBankReader, IDisposable
{
    private readonly FileStream stream;
    private readonly IArchive archive;
    private bool disposed;
    private readonly bool allowDiskFallback;
    private readonly int maximumResourceBytes;
    public VmMusicPatchPaths Paths { get; } = new(allowAbsolutePaths: true);

    public VmGusZipMusicReader(string filename, bool allowDiskFallback = true, int maximumResourceBytes = 0x1fffffff)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumResourceBytes);
        this.maximumResourceBytes = maximumResourceBytes;
        this.allowDiskFallback = allowDiskFallback;
        stream = File.OpenRead(filename);
        try { archive = SharpCompress.Archives.Zip.ZipArchive.OpenArchive(stream); }
        catch { stream.Dispose(); throw; }
    }

    public (byte[]? Data, string Name) Open(string? name)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (name is null) return (OpenFile("timidity.cfg"), "timidity.cfg");
        if (!allowDiskFallback && VmMusicPatchPaths.IsAbsolute(name)) return (null, "");
        return Paths.Lookup(name, OpenFile);
    }

    private byte[]? OpenFile(string name)
    {
        var normalized = name.Replace('\\', '/').Normalize(System.Text.NormalizationForm.FormC);
        var matches = archive.Entries.Where(item => !item.IsDirectory
            && (item.Key ?? "").Replace('\\', '/').Normalize(System.Text.NormalizationForm.FormC)
                .Equals(normalized, StringComparison.OrdinalIgnoreCase));
        var entry = allowDiskFallback ? matches.LastOrDefault() : matches.FirstOrDefault();
        if (entry is not null)
        {
            if (entry.IsEncrypted) throw new InvalidDataException("Encrypted GUS patch resources are unsupported");
            if (entry.Size > maximumResourceBytes)
                throw new InvalidDataException("The GUS archive resource exceeds the size limit.");
            using var data = new LimitedResourceStream(maximumResourceBytes);
            entry.WriteTo(data, new SharpCompress.Common.ExtractionOptions { CheckCrc = true });
            return data.ToArray();
        }
        if (!allowDiskFallback || !File.Exists(name)) return null;
        using var file = File.OpenRead(name);
        if (file.Length > maximumResourceBytes)
            throw new InvalidDataException("The GUS disk resource exceeds the size limit.");
        using var resource = new LimitedResourceStream(maximumResourceBytes);
        file.CopyTo(resource);
        return resource.ToArray();
    }

    private sealed class LimitedResourceStream(int limit) : MemoryStream
    {
        private void CheckSize(int count)
        {
            if (count > limit - Position)
                throw new InvalidDataException("The GUS archive resource exceeds the size limit.");
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckSize(count);
            base.Write(buffer, offset, count);
        }
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            CheckSize(buffer.Length);
            base.Write(buffer);
        }
        public override void WriteByte(byte value)
        {
            CheckSize(1);
            base.WriteByte(value);
        }
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        try { archive.Dispose(); }
        finally { stream.Dispose(); }
    }
}
