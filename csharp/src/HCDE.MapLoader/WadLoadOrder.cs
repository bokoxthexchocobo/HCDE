using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader;

/// <summary>Ordered WAD resources. Later complete map groups override earlier groups.</summary>
public static class WadLoadOrder
{
    public const int MaxBytes = 256 * 1024 * 1024;

    public static bool TryMerge(IReadOnlyList<byte[]> archives, out byte[] merged, out string? error)
    {
        merged = Array.Empty<byte>(); error = null;
        var lumps = new List<(WadLumpEntry Entry, byte[] Archive)>();
        long size = 12;
        foreach (var archive in archives)
        {
            if (!WadArchiveReader.TryReadDirectory(archive, out var entries, out error)) return false;
            foreach (var entry in entries)
            {
                if (!WadArchiveReader.TryReadLumpData(archive, entry, out _, out error)) return false;
                size += entry.Size + 16L;
                if (size > MaxBytes) { error = "wad-load-order-too-large"; return false; }
                lumps.Add((entry, archive));
            }
        }
        merged = new byte[(int)size];
        "PWAD"u8.CopyTo(merged);
        BinaryPrimitives.WriteInt32LittleEndian(merged.AsSpan(4), lumps.Count);
        var directory = (int)size - lumps.Count * 16;
        BinaryPrimitives.WriteInt32LittleEndian(merged.AsSpan(8), directory);
        var cursor = 12;
        foreach (var lump in lumps)
        {
            var length = (int)lump.Entry.Size;
            lump.Archive.AsSpan((int)lump.Entry.FilePosition, length).CopyTo(merged.AsSpan(cursor));
            BinaryPrimitives.WriteInt32LittleEndian(merged.AsSpan(directory), cursor);
            BinaryPrimitives.WriteInt32LittleEndian(merged.AsSpan(directory + 4), length);
            Encoding.ASCII.GetBytes(lump.Entry.Name.AsSpan(), merged.AsSpan(directory + 8, 8));
            cursor += length; directory += 16;
        }
        return true;
    }
}
