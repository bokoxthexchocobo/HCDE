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

    public static bool TryReadHeader(ReadOnlySpan<byte> wad, out WadHeader header, out string? rejectReason) =>
        TryReadHeader(wad, out header, out _, out rejectReason);

    public static bool TryReadHeader(
        ReadOnlySpan<byte> wad,
        out WadHeader header,
        out bool isBigEndian,
        out string? rejectReason)
    {
        header = default;
        isBigEndian = false;
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
        if ((ulong)directoryOffset + (ulong)lumpCount * LumpEntrySize > (ulong)wad.Length)
        {
            lumpCount = BinaryPrimitives.ReadUInt32BigEndian(wad[4..]);
            directoryOffset = BinaryPrimitives.ReadUInt32BigEndian(wad[8..]);
            isBigEndian = true;
            if ((ulong)directoryOffset + (ulong)lumpCount * LumpEntrySize > (ulong)wad.Length)
            {
                rejectReason = "wad-bad-directory-offset";
                return false;
            }
        }

        header = new WadHeader(magic, lumpCount, directoryOffset);
        return true;
    }

    public static bool TryOpenResource(
        ReadOnlySpan<byte> wad,
        out WadOpenResult? result,
        out string? rejectReason,
        IWadArchiveMessageSink? sink = null)
    {
        result = null;
        if (!TryReadDirectory(wad, out var entries, out var metadata, out rejectReason))
            return false;
        WadArchiveNamespaceAssigner.ResetSkinNamespaceCounter();
        WadArchiveNamespaceAssigner.AssignNamespaces(entries, metadata, sink);
        result = new WadOpenResult(entries, metadata);
        return true;
    }

    public static bool TryReadDirectory(ReadOnlySpan<byte> wad, out WadLumpEntry[] entries, out string? rejectReason)
    {
        entries = Array.Empty<WadLumpEntry>();
        rejectReason = null;
        if (!TryReadHeader(wad, out var header, out var isBigEndian, out rejectReason))
            return false;

        var directory = wad[(int)header.DirectoryOffset..];
        if (directory.Length < header.LumpCount * LumpEntrySize)
        {
            rejectReason = "wad-directory-truncated";
            return false;
        }

        entries = new WadLumpEntry[header.LumpCount];
        var lzss = new bool[header.LumpCount];
        var cursor = 0;
        for (var i = 0; i < header.LumpCount; i++)
        {
            var filePosition = ReadUInt32(directory[cursor..], isBigEndian);
            var size = ReadUInt32(directory[(cursor + 4)..], isBigEndian);
            var (name, isLzss) = ReadLumpName(directory[(cursor + 8)..(cursor + 16)]);
            lzss[i] = isLzss;
            entries[i] = new WadLumpEntry(filePosition, size, name);
            cursor += LumpEntrySize;
        }

        for (var index = 0; index < entries.Length; index++)
        {
            if (lzss[index])
            {
                var entry = entries[index];
                var compressedSize = index == entries.Length - 1
                    ? (uint)wad.Length - entry.FilePosition
                    : entries[index + 1].FilePosition - entry.FilePosition;
                entries[index] = new WadLumpEntry(entry.FilePosition, compressedSize, entry.Name);
            }
        }

        return true;
    }

    public static bool TryReadDirectory(
        ReadOnlySpan<byte> wad,
        out WadLumpEntry[] entries,
        out WadLumpMetadata[] metadata,
        out string? rejectReason)
    {
        if (!TryReadDirectory(wad, out entries, out rejectReason))
        {
            metadata = Array.Empty<WadLumpMetadata>();
            return false;
        }

        metadata = new WadLumpMetadata[entries.Length];
        FillLumpMetadata(wad, entries, metadata);
        return true;
    }

    internal static void FillLumpMetadata(ReadOnlySpan<byte> wad, WadLumpEntry[] entries, WadLumpMetadata[] metadata)
    {
        if (!TryReadHeader(wad, out var header, out var isBigEndian, out _))
            return;
        var directory = wad[(int)header.DirectoryOffset..];
        for (var index = 0; index < entries.Length; index++)
        {
            metadata[index] = new WadLumpMetadata();
            var nameBytes = directory[(index * LumpEntrySize + 8)..(index * LumpEntrySize + 16)];
            var highBits = 0;
            for (var byteIndex = 0; byteIndex < 8; byteIndex++)
            {
                if ((nameBytes[byteIndex] & 0x80) != 0)
                    highBits |= 1 << byteIndex;
            }

            metadata[index].HasShortName = true;
            metadata[index].IsLzssCompressed = highBits == 1;
        }
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> data, bool bigEndian) =>
        bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(data) : BinaryPrimitives.ReadUInt32LittleEndian(data);

    public static bool TryReadLumpData(ReadOnlySpan<byte> wad, WadLumpEntry entry, out ReadOnlySpan<byte> data, out string? rejectReason)
    {
        data = default;
        rejectReason = null;
        if ((ulong)entry.FilePosition + entry.Size > (ulong)wad.Length)
        {
            rejectReason = "wad-lump-out-of-range";
            return false;
        }

        data = wad[(int)entry.FilePosition..(int)(entry.FilePosition + entry.Size)];
        return true;
    }

    private static (string Name, bool IsLzss) ReadLumpName(ReadOnlySpan<byte> nameBytes)
    {
        var highBits = 0;
        Span<char> chars = stackalloc char[8];
        var length = 0;
        for (var index = 0; index < 8; index++)
        {
            if ((nameBytes[index] & 0x80) != 0)
                highBits |= 1 << index;
            var ch = (char)(nameBytes[index] & 0x7f);
            if (ch == '\0')
                break;
            chars[length++] = ch;
        }

        if (highBits == 1 && length > 0)
            chars[0] = (char)(chars[0] & 0x7f);

        return (new string(chars[..length]), highBits == 1);
    }
}

public sealed class WadOpenResult
{
    public WadOpenResult(WadLumpEntry[] entries, WadLumpMetadata[] metadata)
    {
        Entries = entries;
        Metadata = metadata;
    }

    public WadLumpEntry[] Entries { get; }
    public WadLumpMetadata[] Metadata { get; }
}
