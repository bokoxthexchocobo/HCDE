using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

public static class VmGrpArchiveReader
{
    public static IReadOnlyList<(string Name, byte[] Data)> Read(byte[] data)
    {
        if (data.Length < 16 || !data.AsSpan().StartsWith("KenSilverman"u8))
            throw new InvalidDataException("Invalid GRP header.");
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(12));
        var position = 16L + count * 16L;
        if (position > data.Length) throw new InvalidDataException("GRP directory exceeds archive bounds.");
        var entries = new List<(string, byte[])>();
        for (var index = 0; index < count; index++)
        {
            var entry = data.AsSpan(16 + index * 16, 16);
            var name = entry[..12];
            var nul = name.IndexOf((byte)0);
            if (nul >= 0) name = name[..nul];
            var size = BinaryPrimitives.ReadUInt32LittleEndian(entry[12..]);
            if (position + size > data.Length) throw new InvalidDataException("GRP entry exceeds archive bounds.");
            entries.Add((new UTF8Encoding(false, true).GetString(name), data.AsSpan((int)position, (int)size).ToArray()));
            position += size;
        }
        return entries.AsReadOnly();
    }
}
