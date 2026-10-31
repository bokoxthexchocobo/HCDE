namespace HCDE.Playsim;

/// <summary>
/// Line breaking, broken-line queries, and the generic UI font switch from
/// <c>v_text.cpp</c> (<b>1–316</b>). Chunk <b>1225</b>.
/// A single-character color code is a glyph during the break and is skipped
/// again by the width measurement. The word-break pointer stays at the first
/// byte of a whitespace run, and a hard break walks back over UTF-8 tails.
/// A newline drops the carried color unless the caller preserves it.
/// </summary>
public sealed class VmTextFont1225
{
    public int Kerning;
    public int Id;
    public bool Complete;
    public Func<int, int> CharWidth = static code => 4;
    public Func<byte[], int> Measure = static text => text.Length == 0 ? 0 : text.Length;
    public Func<string?, bool> CanPrint = static text => true;
}

public sealed class VmBrokenLine1225
{
    public int Width;
    public byte[] Text = Array.Empty<byte>();
}

public sealed class VmBrokenLines1225
{
    public readonly List<VmBrokenLine1225> Lines;

    public VmBrokenLines1225(List<VmBrokenLine1225> lines)
    {
        Lines = lines;
    }

    public int Count()
    {
        return Lines.Count;
    }

    public int WidthAt(int index)
    {
        if ((uint)index >= (uint)Lines.Count)
            return -1;
        return Lines[index].Width;
    }

    public byte[] TextAt(int index)
    {
        if ((uint)index >= (uint)Lines.Count)
            return new byte[] { 0xFF };
        return Lines[index].Text;
    }

    public string Serialize(bool begin)
    {
        if (!begin)
            return "ser-closed";
        var sb = new System.Text.StringBuilder();
        sb.Append("ser-open");
        foreach (VmBrokenLine1225 line in Lines)
        {
            sb.Append(" text=");
            sb.Append(VmText1225.Hex(line.Text));
            sb.Append(" width=");
            sb.Append(line.Width);
        }
        return sb.ToString();
    }
}

public sealed class VmText1225
{
    public const int ColorEscape = 0x1C;

    public List<VmBrokenLine1225> BreakLines(VmTextFont1225 font, int maxWidth, byte[] text, bool preserveColor)
    {
        var lines = new List<VmBrokenLine1225>();
        int i = 0;
        int space = -1;
        int start = 0;
        byte[] last = Array.Empty<byte>();
        byte[] lineColor = Array.Empty<byte>();
        bool lastWasSpace = false;
        int kerning = font.Kerning;
        int w = 0;
        int c;
        while ((c = ReadChar(text, ref i)) != 0)
        {
            if (c == ColorEscape)
            {
                if (At(text, i) != 0)
                {
                    if (At(text, i) == '[')
                    {
                        while (At(text, i) != ']' && At(text, i) != 0)
                            i++;
                        if (At(text, i) != 0)
                            i++;
                    }
                }
                continue;
            }
            if (IsRawSpace(c))
            {
                if (!lastWasSpace)
                {
                    space = i - 1;
                    lastWasSpace = true;
                }
            }
            else
            {
                lastWasSpace = false;
            }
            int nw = font.CharWidth(c);
            if ((w > 0 && w + nw > maxWidth) || c == '\n')
            {
                if (space < 0)
                {
                    space = i - 1;
                    while (space > start && (At(text, space) & 0xC0) == 0x80)
                        space--;
                }
                for (int pos = start; pos < space; pos++)
                {
                    if (At(text, pos) != ColorEscape)
                        continue;
                    pos++;
                    int b = At(text, pos);
                    if (b == 0)
                        continue;
                    if (b == '[')
                    {
                        int cstart = pos;
                        while (At(text, pos) != ']' && At(text, pos) != 0)
                            pos++;
                        if (At(text, pos) != 0)
                            pos++;
                        last = Slice(text, cstart, pos);
                    }
                    else
                    {
                        last = new byte[] { (byte)b };
                        pos++;
                    }
                }
                lines.Add(Make(font, text, start, space, lineColor));
                if (c == '\n' && !preserveColor)
                    last = Array.Empty<byte>();
                lineColor = last;
                w = 0;
                lastWasSpace = false;
                start = space;
                space = -1;
                while (At(text, start) != 0 && IsRawSpace(At(text, start)) && At(text, start) != '\n')
                    start++;
                if (At(text, start) == '\n')
                    start++;
                else
                {
                    while (At(text, start) != 0 && IsRawSpace(At(text, start)))
                        start++;
                }
                i = start;
            }
            else
            {
                w += nw + kerning;
            }
        }
        i--;
        if (i - start >= 1)
        {
            int s = start;
            while (s < i)
            {
                int ch = At(text, s);
                s++;
                if (!IsRawSpace(ch))
                {
                    lines.Add(Make(font, text, start, i, lineColor));
                    break;
                }
            }
        }
        return lines;
    }

    public VmBrokenLines1225 BreakPreserving(VmTextFont1225 font, byte[] text, int maxWidth)
    {
        return new VmBrokenLines1225(BreakLines(font, maxWidth, text, true));
    }

    static VmBrokenLine1225 Make(VmTextFont1225 font, byte[] text, int start, int stop, byte[] lineColor)
    {
        byte[] raw = Slice(text, start, stop);
        byte[] body;
        if (lineColor.Length == 0)
        {
            body = raw;
        }
        else
        {
            body = new byte[1 + lineColor.Length + raw.Length];
            body[0] = ColorEscape;
            System.Buffer.BlockCopy(lineColor, 0, body, 1, lineColor.Length);
            if (raw.Length > 0)
                System.Buffer.BlockCopy(raw, 0, body, 1 + lineColor.Length, raw.Length);
        }
        return new VmBrokenLine1225
        {
            Text = body,
            Width = font.Measure(body),
        };
    }

    public static int At(byte[] text, int index)
    {
        if ((uint)index >= (uint)text.Length)
            return 0;
        return text[index];
    }

    public static int ReadChar(byte[] text, ref int index)
    {
        int z = At(text, index);
        if (z < 192)
        {
            index++;
            if (z >= 128 && z < 160)
                return Win1252[z - 128];
            return z;
        }
        int chr = Utf8Decode(text, index, out int size);
        if (chr >= 0)
        {
            index += size;
            return chr;
        }
        index++;
        return z;
    }

    public static string Hex(byte[] text)
    {
        var sb = new System.Text.StringBuilder(text.Length * 2);
        foreach (byte b in text)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    static bool IsRawSpace(int ch)
    {
        return ch == '\t' || ch == '\r' || ch == '\n' || ch == ' ';
    }

    static byte[] Slice(byte[] text, int start, int stop)
    {
        if (start < 0)
            start = 0;
        if (stop > text.Length)
            stop = text.Length;
        if (stop < start)
            stop = start;
        int n = stop - start;
        if (n <= 0)
            return Array.Empty<byte>();
        var copy = new byte[n];
        System.Buffer.BlockCopy(text, start, copy, 0, n);
        return copy;
    }

    static int Utf8Decode(byte[] text, int baseIndex, out int size)
    {
        int c = At(text, baseIndex);
        size = 1;
        if ((c & 0x80) == 0)
            return c;
        int c1 = At(text, baseIndex + 1);
        if (c1 < 0x80 || c1 >= 0xc0)
            return -1;
        c1 &= 0x3f;
        if ((c & 0xE0) == 0xC0)
        {
            int pair = ((c & 0x1F) << 6) | c1;
            if (pair >= 128)
            {
                size = 2;
                return pair;
            }
            return -1;
        }
        int c2 = At(text, baseIndex + 2);
        if (c2 < 0x80 || c2 >= 0xc0)
            return -1;
        c2 &= 0x3f;
        if ((c & 0xF0) == 0xE0)
        {
            int triple = ((c & 0x0F) << 12) | (c1 << 6) | c2;
            if (triple >= 2048 && (triple < 55296 || triple > 57343))
            {
                size = 3;
                return triple;
            }
            return -1;
        }
        int c3 = At(text, baseIndex + 3);
        if (c3 < 0x80 || c1 >= 0xc0)
            return -1;
        c3 &= 0x3f;
        if ((c & 0xF8) == 0xF0)
        {
            int quad = ((c & 0x07) << 18) | (c1 << 12) | (c2 << 6) | c3;
            if (quad >= 65536 && quad <= 1114111)
            {
                size = 4;
                return quad;
            }
        }
        return -1;
    }

    static readonly int[] Win1252 =
    {
        0x20AC, 0x81, 0x201A, 0x0192, 0x201E, 0x2026, 0x2020, 0x2021,
        0x02C6, 0x2030, 0x0160, 0x2039, 0x0152, 0x8d, 0x017D, 0x8f,
        0x90, 0x2018, 0x2019, 0x201C, 0x201D, 0x2022, 0x2013, 0x2014,
        0x02DC, 0x2122, 0x0161, 0x203A, 0x0153, 0x9d, 0x017E, 0x0178,
    };
}

public sealed class VmGenericUi1225
{
    public VmTextFont1225 NewSmall = new();
    public VmTextFont1225 Small = new();
    public VmTextFont1225? OriginalSmall;
    public VmTextFont1225 Big = new();
    public VmTextFont1225? OriginalBig;
    public VmTextFont1225? AlternativeSmall;
    public VmTextFont1225? AlternativeBig;
    public bool GenericUi;
    public bool SpecialI;
    public int UpperForLowerI = 'I';
    public int LowerForUpperI = 'i';
    public Func<string, string?> Lookup = static name => null;

    public bool CheckFontComplete(VmTextFont1225 font)
    {
        return font.CanPrint(Lookup("REQUIRED_CHARACTERS"));
    }

    public void SetUiGeneric(bool value)
    {
        Update(value);
    }

    public void Update(bool cvar)
    {
        string? switchstr = Lookup("USE_GENERIC_FONT");
        GenericUi = cvar || (switchstr != null && Strtoll(switchstr) != 0);
        AlternativeSmall = NewSmall;
        AlternativeBig = NewSmall;
        if (!GenericUi)
        {
            if (CheckFontComplete(Small))
                AlternativeSmall = Small;
            else if (OriginalSmall != null && CheckFontComplete(OriginalSmall))
                AlternativeSmall = OriginalSmall;
            else
                AlternativeSmall = NewSmall;
            if (CheckFontComplete(Big))
                AlternativeBig = Big;
            else if (OriginalBig != null && CheckFontComplete(OriginalBig))
                AlternativeBig = OriginalBig;
            else
                AlternativeBig = NewSmall;
        }
        switchstr = Lookup("REQUIRED_CHARACTERS");
        SpecialI = switchstr != null && switchstr.IndexOf("\u00C4\u00B0", System.StringComparison.Ordinal) >= 0;
        if (SpecialI)
        {
            UpperForLowerI = 0x130;
            LowerForUpperI = 0x131;
        }
        else
        {
            UpperForLowerI = 'I';
            LowerForUpperI = 'i';
        }
    }

    public static long Strtoll(string text)
    {
        int i = 0;
        int n = text.Length;
        while (i < n && IsCSpace(text[i]))
            i++;
        int sign = 1;
        if (i < n && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        int radix = 10;
        if (i < n && text[i] == '0')
        {
            int p = i + 1;
            if (p < n && (text[p] == 'x' || text[p] == 'X') && p + 1 < n && Digit(text[p + 1], 16) >= 0)
            {
                radix = 16;
                i += 2;
            }
            else
            {
                radix = 8;
            }
        }
        ulong acc = 0;
        bool any = false;
        ulong limit = sign < 0 ? 9223372036854775808UL : 9223372036854775807UL;
        while (i < n)
        {
            int d = Digit(text[i], radix);
            if (d < 0)
                break;
            any = true;
            ulong baseLimit = limit / (ulong)radix;
            if (acc > baseLimit || (acc == baseLimit && (ulong)d > limit % (ulong)radix))
            {
                while (i < n && Digit(text[i], radix) >= 0)
                    i++;
                return sign < 0 ? long.MinValue : long.MaxValue;
            }
            acc = acc * (ulong)radix + (ulong)d;
            i++;
        }
        if (!any)
            return 0;
        if (sign < 0)
            return acc == 9223372036854775808UL ? long.MinValue : -(long)acc;
        return (long)acc;
    }

    static bool IsCSpace(char ch)
    {
        return ch == ' ' || ch == '\t' || ch == '\n' || ch == '\v' || ch == '\f' || ch == '\r';
    }

    static int Digit(char ch, int radix)
    {
        int v;
        if (ch >= '0' && ch <= '9')
            v = ch - '0';
        else if (ch >= 'a' && ch <= 'z')
            v = ch - 'a' + 10;
        else if (ch >= 'A' && ch <= 'Z')
            v = ch - 'A' + 10;
        else
            return -1;
        if (v >= radix)
            return -1;
        return v;
    }
}
