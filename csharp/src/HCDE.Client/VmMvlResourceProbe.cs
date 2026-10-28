using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

/// <summary>Native <c>file_mvl.cpp</c> archive signature probe and directory layout.</summary>
public static class VmMvlResourceProbe
{
    public const int MinimumArchiveLength = 20;

    public static bool IsMvlArchive(ReadOnlySpan<byte> data) =>
        data.Length >= MinimumArchiveLength && data.StartsWith("DMVL"u8);

    public static IReadOnlyList<VmMvlDirectoryEntry> ReadDirectory(ReadOnlySpan<byte> data)
    {
        if (!IsMvlArchive(data))
            throw new InvalidDataException("Not an MVL archive.");
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data[4..]);
        var payloadPosition = 8UL + 17UL * count;
        if (payloadPosition > (ulong)data.Length)
            throw new InvalidDataException("MVL directory exceeds archive bounds.");
        var entries = new List<VmMvlDirectoryEntry>((int)count);
        for (var index = 0; index < (int)count; index++)
        {
            var entry = data.Slice(8 + index * 17, 17);
            var nameBytes = entry[..12];
            var nul = nameBytes.IndexOf((byte)0);
            if (nul >= 0)
                nameBytes = nameBytes[..nul];
            var length = BinaryPrimitives.ReadUInt32LittleEndian(entry[13..]);
            entries.Add(new VmMvlDirectoryEntry(
                NormalizeFileName(nameBytes),
                (ulong)payloadPosition,
                length,
                length));
            payloadPosition += length;
        }

        return entries;
    }

    public static string NormalizeFileName(ReadOnlySpan<byte> nameBytes) =>
        new UTF8Encoding(false, true).GetString(nameBytes);
}

public readonly record struct VmMvlDirectoryEntry(string FileName, ulong Position, uint CompressedSize, uint Length);
