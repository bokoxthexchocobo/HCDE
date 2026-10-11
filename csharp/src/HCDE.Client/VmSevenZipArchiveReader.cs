using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using SharpCompress.Common;

namespace HCDE.Client;

internal sealed class VmSevenZipArchiveReader : IDisposable
{
    private readonly MemoryStream _input;
    private readonly IArchive _archive;

    public VmSevenZipArchiveReader(byte[] data)
    {
        if (data.Length < 32 || !data.AsSpan().StartsWith(new byte[] { 55, 122, 188, 175, 39, 28 }))
            throw new InvalidDataException("Invalid 7z header.");
        _input = new MemoryStream(data, writable: false);
        try { _archive = SevenZipArchive.OpenArchive(_input); }
        catch { _input.Dispose(); throw; }
    }

    public IReadOnlyList<(string Name, Func<byte[]> Read)> GetEntries()
    {
        var entries = new List<(string, Func<byte[]>)>();
        foreach (var entry in _archive.Entries)
        {
            if (entry.IsDirectory || string.IsNullOrEmpty(entry.Key)) continue;
            if (entry.Size > int.MaxValue) throw new InvalidDataException("7z entry exceeds supported size.");
            entries.Add((entry.Key, () => ReadEntry(entry)));
        }
        // Native Open validates the first file's compression before applying filters.
        if (entries.Count != 0)
        {
            var first = entries[0];
            var validated = first.Item2();
            entries[0] = (first.Item1, () => validated);
        }
        return entries.AsReadOnly();
    }

    private static byte[] ReadEntry(IArchiveEntry entry)
    {
        using var output = new MemoryStream();
        entry.WriteTo(output, new ExtractionOptions { CheckCrc = true });
        if (output.Length != entry.Size) throw new InvalidDataException("Truncated 7z entry.");
        return output.ToArray();
    }

    public void Dispose()
    {
        try { _archive.Dispose(); }
        finally { _input.Dispose(); }
    }
}
