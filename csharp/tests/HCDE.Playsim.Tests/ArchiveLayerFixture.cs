using System.Buffers.Binary;

namespace HCDE.Playsim.Tests;

internal static class ArchiveLayerFixture
{
    internal static int TrailerStart(byte[] bytes, int wantedVersion)
    {
        if (bytes.Length < 16) throw new ArgumentException("Truncated archive.", nameof(bytes));
        var version = (int)BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(4));
        var end = bytes.Length;
        while (version >= 16)
        {
            var size = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(end - 4));
            if (size < 8 || size > end - 16) throw new ArgumentException("Invalid archive layer size.", nameof(bytes));
            var start = end - size;
            if (version == wantedVersion) return start;
            var prior = BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(start));
            if (prior < 15 || prior >= version) throw new ArgumentException("Invalid archive layer version.", nameof(bytes));
            version = prior;
            end = start;
        }
        throw new ArgumentException($"Archive layer {wantedVersion} was not found.", nameof(wantedVersion));
    }
}
