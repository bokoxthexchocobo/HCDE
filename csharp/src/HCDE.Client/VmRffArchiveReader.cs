using System.Buffers.Binary;
using System.Text;

namespace HCDE.Client;

internal static class VmRffArchiveReader
{
    public static IReadOnlyList<(string Name, byte[] Data)> Read(byte[] data)
    {
        if (data.Length < 16 || !data.AsSpan().StartsWith("RFF\u001a"u8))
            throw new InvalidDataException("Invalid RFF header.");
        var offset = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(8));
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(12));
        var directorySize = (ulong)count * 48;
        if ((ulong)offset + directorySize > (ulong)data.Length)
            throw new InvalidDataException("RFF directory exceeds archive bounds.");
        var directory = data.AsSpan((int)offset, (int)directorySize).ToArray();
        for (var index = 0; index < directory.Length; index++)
            directory[index] ^= unchecked((byte)(offset + (uint)(index >> 1)));
        var entries = new List<(string, byte[])>();
        var encoding = new UTF8Encoding(false, true);
        for (var index = 0; index < (int)count; index++)
        {
            var entry = directory.AsSpan(index * 48, 48);
            var position = BinaryPrimitives.ReadUInt32LittleEndian(entry[16..]);
            var length = BinaryPrimitives.ReadUInt32LittleEndian(entry[20..]);
            if ((ulong)position + length > (ulong)data.Length)
                throw new InvalidDataException("RFF entry exceeds archive bounds.");
            var name = entry.Slice(36, 8);
            var nul = name.IndexOf((byte)0);
            if (nul >= 0) name = name[..nul];
            var fullName = encoding.GetString(name) + "." + encoding.GetString(entry.Slice(33, 3));
            var terminator = fullName.IndexOf('\0');
            if (terminator >= 0) fullName = fullName[..terminator];
            var payload = data.AsSpan((int)position, (int)length).ToArray();
            if ((entry[32] & 0x10) != 0)
                for (var byteIndex = 0; byteIndex < Math.Min(payload.Length, 256); byteIndex++)
                    payload[byteIndex] ^= (byte)(byteIndex >> 1);
            entries.Add((fullName, payload));
        }
        return entries.AsReadOnly();
    }
}
