using System.Buffers.Binary;
using System.Globalization;

namespace HCDE.Client;

internal static class VmWhresArchiveReader
{
    public static bool HasDirectory(byte[] data)
    {
        if (data.Length < 8192) return false;
        var directory = data.AsSpan(data.Length - 4096);
        var expectedBlock = 0U;
        for (var index = 0; index < 341; index++)
        {
            var block = BinaryPrimitives.ReadUInt32LittleEndian(directory[(index * 12)..]);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(directory[(index * 12 + 4)..]);
            if (length == 0 && block == 0) break;
            // Native detection compares block numbers plus byte lengths; preserve that check.
            if (block != expectedBlock || length == 0 || unchecked(block + length) >= (ulong)(data.Length - 4096)) return false;
            expectedBlock = unchecked(expectedBlock + unchecked(length + 4095) / 4096);
        }
        return true;
    }

    public static IReadOnlyList<(string Name, byte[] Data)> Read(string archiveName, byte[] data)
    {
        if (!HasDirectory(data)) throw new InvalidDataException("Invalid WHRES directory.");
        var basename = archiveName[(archiveName.LastIndexOfAny(['/', '\\']) + 1)..];
        var dot = basename.LastIndexOf('.');
        if (dot >= 0) basename = basename[..dot];
        var directory = data.AsSpan(data.Length - 4096);
        var entries = new List<(string, byte[])>();
        for (var index = 0; index < 341; index++)
        {
            var block = BinaryPrimitives.ReadUInt32LittleEndian(directory[(index * 12)..]);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(directory[(index * 12 + 4)..]);
            if (length == 0) break;
            var position = (ulong)block * 4096;
            if (position + length > (ulong)(data.Length - 4096))
                throw new InvalidDataException("WHRES entry exceeds payload bounds.");
            entries.Add((basename + "/" + index.ToString("D4", CultureInfo.InvariantCulture),
                data.AsSpan((int)position, (int)length).ToArray()));
        }
        return entries.AsReadOnly();
    }
}
