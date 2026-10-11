using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

internal static class VmMvlArchiveReader
{
    public static IReadOnlyList<(string Name, byte[] Data)> Read(byte[] data)
    {
        if (data.Length < 20 || !data.AsSpan().StartsWith("DMVL"u8))
            throw new InvalidDataException("Invalid MVL header.");
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(4));
        var payloadPosition = 8UL + 17UL * count;
        if (payloadPosition > (ulong)data.Length)
            throw new InvalidDataException("MVL directory exceeds archive bounds.");
        var entries = new List<(string, byte[])>();
        var encoding = new UTF8Encoding(false, true);
        for (var index = 0; index < (int)count; index++)
        {
            var entry = data.AsSpan(8 + index * 17, 17);
            var name = entry[..12];
            var nul = name.IndexOf((byte)0);
            if (nul >= 0) name = name[..nul];
            var length = BinaryPrimitives.ReadUInt32LittleEndian(entry[13..]);
            if (payloadPosition + length > (ulong)data.Length)
                throw new InvalidDataException("MVL entry exceeds archive bounds.");
            entries.Add((encoding.GetString(name), data.AsSpan((int)payloadPosition, (int)length).ToArray()));
            payloadPosition += length;
        }
        return entries.AsReadOnly();
    }
}
