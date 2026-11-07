namespace HCDE.Playsim;

/// <summary>
/// Filename UTF conversion from <c>unicode.cpp</c> 1-150. Chunk 1405.
/// The case-fold map stays a callee. The file ends at line 150.
/// </summary>
public static class VmUnicode1405Slice
{
    public const int BandStart = 1;
    public const int BandEnd = 150;
    public const int NullTerm = 1 << 0;
    public const int Stable = 1 << 1;
    public const int Compose = 1 << 3;
    public const int CaseFold = 1 << 10;
    public const int FoldOptions = NullTerm | Stable | Compose | CaseFold;

    public static readonly int[] Ibm437 =
    [
        0x00C7, 0x00FC, 0x00E9, 0x00E2, 0x00E4, 0x00E0, 0x00E5, 0x00E7, 0x00EA, 0x00EB, 0x00E8, 0x00EF, 0x00EE, 0x00EC, 0x00C4, 0x00C5,
        0x00C9, 0x00E6, 0x00C6, 0x00F4, 0x00F6, 0x00F2, 0x00FB, 0x00F9, 0x00FF, 0x00D6, 0x00DC, 0x00A2, 0x00A3, 0x00A5, 0x20A7, 0x0192,
        0x00E1, 0x00ED, 0x00F3, 0x00FA, 0x00F1, 0x00D1, 0x00AA, 0x00BA, 0x00BF, 0x2310, 0x00AC, 0x00BD, 0x00BC, 0x00A1, 0x00AB, 0x00BB,
        0x2591, 0x2592, 0x2593, 0x2502, 0x2524, 0x2561, 0x2562, 0x2556, 0x2555, 0x2563, 0x2551, 0x2557, 0x255D, 0x255C, 0x255B, 0x2510,
        0x2514, 0x2534, 0x252C, 0x251C, 0x2500, 0x253C, 0x255E, 0x255F, 0x255A, 0x2554, 0x2569, 0x2566, 0x2560, 0x2550, 0x256C, 0x2567,
        0x2568, 0x2564, 0x2565, 0x2559, 0x2558, 0x2552, 0x2553, 0x256B, 0x256A, 0x2518, 0x250C, 0x2588, 0x2584, 0x258C, 0x2590, 0x2580,
        0x03B1, 0x00DF, 0x0393, 0x03C0, 0x03A3, 0x03C3, 0x00B5, 0x03C4, 0x03A6, 0x0398, 0x03A9, 0x03B4, 0x221E, 0x03C6, 0x03B5, 0x2229,
        0x2261, 0x00B1, 0x2265, 0x2264, 0x2320, 0x2321, 0x00F7, 0x2248, 0x00B0, 0x2219, 0x00B7, 0x221A, 0x207F, 0x00B2, 0x25A0, 0x00A0,
    ];

    public static void Encode(int codepoint, List<byte> buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (codepoint < 0 || codepoint > 0x10FFFF || (codepoint >= 0xD800 && codepoint < 0xDFFF))
            codepoint = 0xFFFD;
        if (codepoint < 0x80)
        {
            buffer.Add((byte)codepoint);
        }
        else if (codepoint < 0x800)
        {
            buffer.Add((byte)(0xC0 + (codepoint >> 6)));
            buffer.Add((byte)(0x80 + (codepoint & 0x3F)));
        }
        else if (codepoint < 0x10000)
        {
            buffer.Add((byte)(0xE0 + (codepoint >> 12)));
            buffer.Add((byte)(0x80 + ((codepoint >> 6) & 0x3F)));
            buffer.Add((byte)(0x80 + (codepoint & 0x3F)));
        }
        else if (codepoint < 0x110000)
        {
            buffer.Add((byte)(0xF0 + (codepoint >> 18)));
            buffer.Add((byte)(0x80 + ((codepoint >> 12) & 0x3F)));
            buffer.Add((byte)(0x80 + ((codepoint >> 6) & 0x3F)));
            buffer.Add((byte)(0x80 + (codepoint & 0x3F)));
        }
    }

    public static void Utf16ToUtf8(ReadOnlySpan<ushort> units, List<byte> buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        buffer.Clear();
        if (units.IsEmpty || units[0] == 0)
            return;
        int index = 0;
        while (index < units.Length && units[index] != 0)
        {
            int unit = units[index++];
            if (unit >= 0xD800 && unit <= 0xDBFF)
            {
                int next = index < units.Length ? units[index] : 0;
                if (next >= 0xDC00 && next <= 0xDFFF)
                {
                    index++;
                    unit -= 0xD800;
                    next -= 0xDC00;
                    unit <<= 10;
                    unit += next;
                    unit += 0x10000;
                }
                else
                {
                    unit = 0xFFFD;
                }
            }
            else if (unit >= 0xDC00 && unit <= 0xDFFF)
            {
                unit = 0xFFFD;
            }
            Encode(unit, buffer);
        }
        buffer.Add(0);
    }

    public static void Ibm437ToUtf8(ReadOnlySpan<byte> text, List<byte> buffer)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        buffer.Clear();
        if (text.IsEmpty || text[0] == 0)
            return;
        int index = 0;
        while (index < text.Length && text[index] != 0)
        {
            int unit = text[index++];
            if (unit >= 0x80)
                unit = Ibm437[unit - 0x80];
            Encode(unit, buffer);
        }
        buffer.Add(0);
    }

    public static byte[] Fold(byte[] text, Func<byte[], int, byte[]> map)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        if (map == null)
            throw new ArgumentNullException(nameof(map));
        return map(text, FoldOptions);
    }

    public static bool Validate(byte[] text, Func<byte[], int, int> iterate)
    {
        if (text == null)
            throw new ArgumentNullException(nameof(text));
        if (iterate == null)
            throw new ArgumentNullException(nameof(iterate));
        int index = 0;
        while (index < text.Length && text[index] != 0)
        {
            int result = iterate(text, index);
            if (result < 0)
                return false;
        }
        return true;
    }
}
