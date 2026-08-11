namespace HCDE.Net.Transport;

/// <summary>
/// zlib-compatible CRC-32 matching <c>CalcCRC32</c> / <c>AddCRC32</c> in <c>m_crc32.h</c>.
/// </summary>
public static class Crc32
{
    private static readonly uint[] Table = CreateTable();

    public static uint Calc(ReadOnlySpan<byte> data) => Add(0, data);

    public static uint Add(uint crc, ReadOnlySpan<byte> data)
    {
        crc ^= 0xFFFFFFFF;
        foreach (var b in data)
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFF;
    }

    private static uint[] CreateTable()
    {
        var table = new uint[256];
        for (var i = 0; i < 256; i++)
        {
            var remainder = (uint)i;
            for (var j = 0; j < 8; j++)
                remainder = (remainder & 1) != 0 ? (remainder >> 1) ^ 0xEDB88320 : remainder >> 1;
            table[i] = remainder;
        }

        return table;
    }
}
