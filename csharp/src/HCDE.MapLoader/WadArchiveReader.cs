using System.Buffers.Binary;
using System.Text;

namespace HCDE.MapLoader;

public readonly struct WadHeader
{
    public WadHeader(WadType type, uint lumpCount, uint directoryOffset)
    {
        Type = type;
        LumpCount = lumpCount;
        DirectoryOffset = directoryOffset;
    }

    public WadType Type { get; }
    public uint LumpCount { get; }
    public uint DirectoryOffset { get; }
}

public readonly struct WadLumpEntry
{
    public WadLumpEntry(uint filePosition, uint size, string name)
    {
        FilePosition = filePosition;
        Size = size;
        Name = name;
    }

    public uint FilePosition { get; }
    public uint Size { get; }
    public string Name { get; }
}

public static class WadArchiveReader
{
    public const int HeaderSize = 12;
    public const int LumpEntrySize = 16;

    public static bool TryReadHeader(ReadOnlySpan<byte> wad, out WadHeader header, out string? rejectReason)
    {
        header = default;
        rejectReason = null;
        if (wad.Length < HeaderSize)
        {
            rejectReason = "wad-too-small";
            return false;
        }

        var magic = (WadType)BinaryPrimitives.ReadUInt32LittleEndian(wad);
        if (magic is not (WadType.Iwad or WadType.Pwad))
        {
            rejectReason = "wad-bad-magic";
            return false;
        }

        var lumpCount = BinaryPrimitives.ReadUInt32LittleEndian(wad[4..]);
        var directoryOffset = BinaryPrimitives.ReadUInt32LittleEndian(wad[8..]);
        if (directoryOffset + lumpCount * LumpEntrySize > (uint)wad.Length)
        {
            rejectReason = "wad-directory-out-of-range";
            return false;
        }

        header = new WadHeader(magic, lumpCount, directoryOffset);
        return true;
    }

    public static bool TryReadDirectory(ReadOnlySpan<byte> wad, out WadLumpEntry[] entries, out string? rejectReason)
    {
        entries = Array.Empty<WadLumpEntry>();
        rejectReason = null;
        if (!TryReadHeader(wad, out var header, out rejectReason))
            return false;

        var directory = wad[(int)header.DirectoryOffset..];
        if (directory.Length < header.LumpCount * LumpEntrySize)
        {
            rejectReason = "wad-directory-truncated";
            return false;
        }

        entries = new WadLumpEntry[header.LumpCount];
        var cursor = 0;
        for (var i = 0; i < header.LumpCount; i++)
        {
            var filePosition = BinaryPrimitives.ReadUInt32LittleEndian(directory[cursor..]);
            var size = BinaryPrimitives.ReadUInt32LittleEndian(directory[(cursor + 4)..]);
            var name = ReadLumpName(directory[(cursor + 8)..(cursor + 16)]);
            entries[i] = new WadLumpEntry(filePosition, size, name);
            cursor += LumpEntrySize;
        }

        return true;
    }

    public static bool TryReadLumpData(ReadOnlySpan<byte> wad, WadLumpEntry entry, out ReadOnlySpan<byte> data, out string? rejectReason)
    {
        data = default;
        rejectReason = null;
        if (entry.FilePosition + entry.Size > (uint)wad.Length)
        {
            rejectReason = "wad-lump-out-of-range";
            return false;
        }

        data = wad[(int)entry.FilePosition..(int)(entry.FilePosition + entry.Size)];
        return true;
    }

    private static string ReadLumpName(ReadOnlySpan<byte> nameBytes)
    {
        var end = nameBytes.IndexOf((byte)0);
        if (end < 0)
            end = nameBytes.Length;

        return Encoding.ASCII.GetString(nameBytes[..end]).TrimEnd();
    }
}
