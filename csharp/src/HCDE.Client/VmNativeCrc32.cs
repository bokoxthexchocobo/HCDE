using System.Text;

namespace HCDE.Client;

/// <summary><c>CalcCRC32</c> from <c>m_crc32.h</c> (string helper for stringtable).</summary>
public static class VmNativeCrc32
{
    private static readonly uint[] Table = CreateTable();

    public static uint Calc(string text) => Calc(Encoding.UTF8.GetBytes(text));

    public static uint Calc(ReadOnlySpan<byte> data)
    {
        var crc = 0xFFFFFFFFu;
        foreach (var b in data)
            crc = Table[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc ^ 0xFFFFFFFFu;
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
