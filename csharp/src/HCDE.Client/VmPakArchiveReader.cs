using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

public static class VmPakArchiveReader
{
    public static IReadOnlyList<(string Name, byte[] Data)> Read(byte[] data)
    {
        if (data.Length < 12 || !data.AsSpan().StartsWith("PACK"u8)) throw new InvalidDataException("Invalid PAK header.");
        var offset = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(4));
        var size = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(8));
        if ((ulong)offset + size > (ulong)data.Length) throw new InvalidDataException("PAK directory exceeds archive bounds.");
        var entries = new List<(string, byte[])>();
        for (var index = 0U; index < size / 64; index++)
        {
            var entry = data.AsSpan((int)(offset + index * 64), 64);
            var name = entry[..56];
            var nul = name.IndexOf((byte)0);
            if (nul >= 0) name = name[..nul];
            var position = BinaryPrimitives.ReadUInt32LittleEndian(entry[56..]);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(entry[60..]);
            if ((ulong)position + length > (ulong)data.Length) throw new InvalidDataException("PAK entry exceeds archive bounds.");
            entries.Add((new UTF8Encoding(false, true).GetString(name), data.AsSpan((int)position, (int)length).ToArray()));
        }
        return entries.AsReadOnly();
    }
}
