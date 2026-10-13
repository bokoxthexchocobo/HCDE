using SharpCompress.Archives;

namespace HCDE.Client;

public sealed class VmGusZipMusicReader : IVmMusicBankReader, IDisposable
{
    private readonly FileStream stream;
    private readonly IArchive archive;
    private bool disposed;
    public VmMusicPatchPaths Paths { get; } = new(allowAbsolutePaths: true);

    public VmGusZipMusicReader(string filename)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filename);
        stream = File.OpenRead(filename);
        try { archive = SharpCompress.Archives.Zip.ZipArchive.OpenArchive(stream); }
        catch { stream.Dispose(); throw; }
    }

    public (byte[]? Data, string Name) Open(string? name)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (name is null) return (OpenFile("timidity.cfg"), "timidity.cfg");
        return Paths.Lookup(name, OpenFile);
    }

    private byte[]? OpenFile(string name)
    {
        var normalized = name.Replace('\\', '/').Normalize(System.Text.NormalizationForm.FormC);
        var entry = archive.Entries.LastOrDefault(item => !item.IsDirectory
            && (item.Key ?? "").Replace('\\', '/').Normalize(System.Text.NormalizationForm.FormC)
                .Equals(normalized, StringComparison.OrdinalIgnoreCase));
        if (entry is not null)
        {
            if (entry.IsEncrypted) throw new InvalidDataException("Encrypted GUS patch resources are unsupported");
            using var data = new MemoryStream();
            entry.WriteTo(data, new SharpCompress.Common.ExtractionOptions { CheckCrc = true });
            return data.ToArray();
        }
        return File.Exists(name) ? File.ReadAllBytes(name) : null;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        archive.Dispose();
        stream.Dispose();
    }
}
