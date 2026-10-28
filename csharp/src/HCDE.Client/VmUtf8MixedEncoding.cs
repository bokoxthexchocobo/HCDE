namespace HCDE.Client;

/// <summary><c>GetCharFromString</c> / Windows-1252 map from <c>utf8.cpp</c>.</summary>
public static class VmUtf8MixedEncoding
{
    private static readonly ushort[] Win1252Map =
    [
        0x20AC, 0x81, 0x201A, 0x0192, 0x201E, 0x2026, 0x2020, 0x2021,
        0x02C6, 0x2030, 0x0160, 0x2039, 0x0152, 0x8D, 0x017D, 0x8F,
        0x90, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014,
        0x02DC, 0x2122, 0x0161, 0x203A, 0x0153, 0x9D, 0x017E, 0x0178,
    ];

    public static int GetCharFromString(ref ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty) return 0;
        var z = data[0];
        if (z < 192)
        {
            data = data[1..];
            if (z >= 128 && z < 160) return Win1252Map[z - 128];
            return z;
        }

        if (VmUtf8Codec.TryDecode(data, out var chr, out var size) && chr >= 0)
        {
            data = data[size..];
            return chr;
        }

        data = data[1..];
        return z;
    }

    public static string MakeUtf8(string outline)
    {
        ReadOnlySpan<byte> bytes = System.Text.Encoding.Latin1.GetBytes(outline);
        var builder = new System.Text.StringBuilder();
        var chars = 0;
        while (bytes.Length > 0)
        {
            var chr = GetCharFromString(ref bytes);
            if (chr == 0) break;
            chars++;
            builder.Append(char.ConvertFromUtf32(chr));
        }

        return builder.ToString();
    }
}
