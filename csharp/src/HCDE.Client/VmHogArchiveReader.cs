using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

internal static class VmHogArchiveReader
{
    public static IReadOnlyList<(string Name, byte[] Data)> Read(byte[] data)
    {
        if (data.Length < 20 || !data.AsSpan().StartsWith("DHF"u8))
            throw new InvalidDataException("Invalid HOG header.");
        var entries = new List<(string, byte[])>();
        var encoding = new UTF8Encoding(false, true);
        var position = 3;
        // Native scanning ignores a trailing fragment shorter than the name field.
        while (data.Length - position >= 13)
        {
            if (data.Length - position < 17)
                throw new InvalidDataException("Truncated HOG record length.");
            var name = data.AsSpan(position, 12);
            var nul = name.IndexOf((byte)0);
            if (nul >= 0) name = name[..nul];
            var length = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(position + 13));
            position += 17;
            if ((ulong)position + length > (ulong)data.Length)
                throw new InvalidDataException("HOG entry exceeds archive bounds.");
            entries.Add((encoding.GetString(name), data.AsSpan(position, (int)length).ToArray()));
            position += (int)length;
        }
        return entries.AsReadOnly();
    }
}
