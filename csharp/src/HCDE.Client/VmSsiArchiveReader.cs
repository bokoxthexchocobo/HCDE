using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

internal static class VmSsiArchiveReader
{
    public static bool HasHeader(byte[] data)
    {
        if (data.Length < 8) return false;
        var version = BinaryPrimitives.ReadInt32LittleEndian(data);
        if (version is not (1 or 2)) return false;
        var position = 8;
        if (!SkipString(data, ref position, 32)) return false;
        if (version == 2 && !SkipString(data, ref position, 12)) return false;
        for (var index = 0; index < 3; index++)
            if (!SkipString(data, ref position, 70)) return false;
        return true;
    }

    private static bool SkipString(byte[] data, ref int position, int size)
    {
        if (data.Length - position < size + 1) return false;
        var length = data[position++];
        var field = data.AsSpan(position, size);
        position += size;
        if (length > size || field[..length].Contains((byte)0)) return false;
        foreach (var value in field[length..]) if (value != 0) return false;
        return true;
    }

    public static IReadOnlyList<(string Name, byte[] Data, bool Embedded)> Read(byte[] data)
    {
        if (!HasHeader(data)) throw new InvalidDataException("Invalid SSI header.");
        var version = BinaryPrimitives.ReadInt32LittleEndian(data);
        var count = BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(4));
        var headerSize = version == 2 ? 267 : 254;
        var position = (long)headerSize + (long)count * 121;
        if (count < 0 || position > data.Length) throw new InvalidDataException("SSI directory exceeds archive bounds.");
        var entries = new List<(string, byte[], bool)>();
        var encoding = new UTF8Encoding(false, true);
        for (var index = 0; index < count; index++)
        {
            var entry = data.AsSpan(headerSize + index * 121, 121);
            var nameLength = Math.Min((int)entry[0], 12);
            if (nameLength < 3) throw new InvalidDataException("SSI name is too short for its extension alias.");
            var name = entry.Slice(1, nameLength).ToArray();
            var length = BinaryPrimitives.ReadInt32LittleEndian(entry[13..]);
            if (length < 0 || position + length > data.Length) throw new InvalidDataException("SSI entry exceeds archive bounds.");
            var payload = data.AsSpan((int)position, length).ToArray();
            AddName(name, payload);
            (name[^1], name[^3]) = (name[^3], name[^1]);
            AddName(name, payload);
            position += length;
        }
        return entries.AsReadOnly();

        void AddName(byte[] name, byte[] payload)
        {
            var nul = Array.IndexOf(name, (byte)0);
            var text = encoding.GetString(name, 0, nul < 0 ? name.Length : nul);
            entries.Add((text, payload, text.Contains(".GRP", StringComparison.Ordinal)));
        }
    }
}
