using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace HCDE.MapLoader;

public sealed record ModResource(string Path, ReadOnlyMemory<byte> Data);

/// <summary>Ordered in-memory WAD/PK3 resources. Never extracts archive paths to disk.</summary>
public sealed class ModResources
{
    private readonly List<ModResource> _entries = new();
    private readonly List<(string Name, byte[] Data)> _lumps = new();
    private long _expandedBytes;
    public IReadOnlyList<ModResource> Entries => _entries.AsReadOnly();
    public byte[] MapWad { get; private set; } = [];

    public ModResource? Find(string path) => _entries.LastOrDefault(entry => entry.Path.Equals(path.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));

    public static bool TryLoad(IReadOnlyList<byte[]> archives, out ModResources resources, out string? error)
    {
        resources = new ModResources(); error = null;
        try
        {
            if (archives.Sum(archive => (long)archive.Length) > WadLoadOrder.MaxBytes)
                throw new InvalidDataException("mod-input-too-large");
            foreach (var archive in archives)
            {
                if (archive.AsSpan().StartsWith("IWAD"u8) || archive.AsSpan().StartsWith("PWAD"u8)) resources.AddWad(archive);
                else resources.AddZip(archive);
            }
            resources.MapWad = resources.BuildWad();
            return true;
        }
        catch (Exception exception) when (exception is InvalidDataException or IOException or NotSupportedException or ArgumentException or OverflowException)
        {
            resources = new ModResources(); error = exception.Message;
            return false;
        }
    }

    private void Charge(long bytes)
    {
        _expandedBytes = checked(_expandedBytes + bytes);
        if (_expandedBytes > WadLoadOrder.MaxBytes || _entries.Count >= 65536)
            throw new InvalidDataException("mod-expanded-size-or-entry-limit");
    }

    private void AddWad(byte[] bytes, string? mapName = null)
    {
        if (!WadArchiveReader.TryReadHeader(bytes, out var header, out var error)) throw new InvalidDataException(error);
        if (header.LumpCount > 65536 - _entries.Count) throw new InvalidDataException("mod-entry-limit");
        if (!WadArchiveReader.TryReadDirectory(bytes, out var directory, out error)) throw new InvalidDataException(error);
        for (var index = 0; index < directory.Length; index++)
        {
            var entry = directory[index];
            if (!WadArchiveReader.TryReadLumpData(bytes, entry, out var data, out error)) throw new InvalidDataException(error);
            Charge(data.Length + 16L);
            var copy = data.ToArray();
            var name = index == 0 && mapName != null ? mapName : entry.Name;
            _entries.Add(new ModResource(name, copy));
            _lumps.Add((name, copy));
        }
    }

    private void AddZip(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes, writable: false);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
        if (archive.Entries.Count > 65536) throw new InvalidDataException("mod-entry-limit");
        foreach (var entry in archive.Entries)
        {
            var path = entry.FullName.Replace('\\', '/');
            if (path.StartsWith('/') || path.Contains(':') || path.Split('/').Any(part => part is ".." or "."))
                throw new InvalidDataException("mod-invalid-resource-path");
            if (path.EndsWith('/')) continue;
            Charge(entry.Length + 16L);
            var data = new byte[checked((int)entry.Length)];
            using (var input = entry.Open())
            {
                input.ReadExactly(data);
                if (input.ReadByte() != -1) throw new InvalidDataException("mod-entry-size-mismatch");
            }
            _entries.Add(new ModResource(path, data));
            if (path.StartsWith("maps/", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".wad", StringComparison.OrdinalIgnoreCase))
            {
                var mapName = Path.GetFileNameWithoutExtension(path);
                if (mapName.Length is < 1 or > 8 || mapName.Any(ch => ch > 127)) throw new InvalidDataException("mod-map-name-invalid");
                AddWad(data, mapName);
            }
            else if (!path.Contains('/'))
            {
                var name = Path.GetFileNameWithoutExtension(path);
                if (name.Length is > 0 and <= 8 && name.All(ch => ch <= 127)) _lumps.Add((name, data));
            }
        }
    }

    private byte[] BuildWad()
    {
        var size = 12L + _lumps.Sum(lump => 16L + lump.Data.Length);
        if (size > WadLoadOrder.MaxBytes) throw new InvalidDataException("mod-map-wad-too-large");
        var bytes = new byte[(int)size];
        "PWAD"u8.CopyTo(bytes);
        var directory = bytes.Length - _lumps.Count * 16;
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(4), _lumps.Count);
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8), directory);
        var cursor = 12;
        foreach (var lump in _lumps)
        {
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(directory), cursor);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(directory + 4), lump.Data.Length);
            Encoding.ASCII.GetBytes(lump.Name.AsSpan(), bytes.AsSpan(directory + 8, 8));
            lump.Data.CopyTo(bytes, cursor);
            cursor += lump.Data.Length; directory += 16;
        }
        return bytes;
    }
}
