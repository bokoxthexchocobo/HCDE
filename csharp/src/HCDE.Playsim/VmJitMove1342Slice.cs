using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// JIT register moves, casts, and dynamic class casts from <c>jit_move.cpp</c>
/// (1-312). Chunk 1342. String formatting is <c>stb_sprintf</c>, integer and
/// double parses are <c>strtoll</c> base 0 and <c>strtod</c>, and name, color,
/// sound, sprite, and texture lookups stay callees.
/// </summary>
public sealed class VmJitMove1342
{
    public const int NameNone = 0;
    public const int CastI2F = 0;
    public const int CastI2S = 1;
    public const int CastU2F = 2;
    public const int CastU2S = 3;
    public const int CastF2I = 4;
    public const int CastF2U = 5;
    public const int CastF2S = 6;
    public const int CastP2S = 7;
    public const int CastS2I = 8;
    public const int CastS2F = 9;
    public const int CastS2N = 10;
    public const int CastN2S = 11;
    public const int CastS2Co = 12;
    public const int CastS2So = 13;
    public const int CastCo2S = 14;
    public const int CastSo2S = 15;
    public const int CastV22S = 16;
    public const int CastV32S = 17;
    public const int CastV42S = 18;
    public const int CastSid2S = 19;
    public const int CastTid2S = 20;
    public const int CastBInt = 21;
    public const int CastBFloat = 22;
    public const int CastBPointer = 23;
    public const int CastBString = 24;

    public readonly int[] RegD = new int[260];
    public readonly double[] RegF = new double[260];
    public readonly string?[] RegS = new string?[260];
    public readonly long[] RegA = new long[260];
    public readonly long[] KonstA = new long[256];

    public byte A;
    public byte B;
    public byte C;
    public int NumNames;
    public Func<string, int>? NameIndex;
    public Func<int, string>? NameChars;
    public Func<string, int>? ColorOf;
    public Func<string, int>? SoundIndex;
    public Func<int, string>? SoundName;
    public Func<uint, string>? SpriteString;
    public Func<int, string?>? TextureName;
    public Func<long, long, bool>? IsKindOf;
    public Func<long, long, bool>? IsDescendantOf;

    public VmJitMove1342()
    {
        for (int i = 0; i < RegS.Length; i++)
            RegS[i] = "";
    }

    public void EmitMOVE()
    {
        RegD[A] = RegD[B];
    }

    public void EmitMOVEF()
    {
        RegF[A] = RegF[B];
    }

    public void EmitMOVES()
    {
        RegS[A] = SourceString(B);
    }

    public void EmitMOVEA()
    {
        RegA[A] = RegA[B];
    }

    public void EmitMOVEV2()
    {
        CopyFloats(2);
    }

    public void EmitMOVEV3()
    {
        CopyFloats(3);
    }

    public void EmitMOVEV4()
    {
        CopyFloats(4);
    }

    public void EmitCAST()
    {
        switch (C)
        {
            case CastI2F:
                RegF[A] = RegD[B];
                break;
            case CastU2F:
                RegF[A] = unchecked((uint)RegD[B]);
                break;
            case CastF2I:
                RegD[A] = TruncateToInt32(RegF[B]);
                break;
            case CastF2U:
                RegD[A] = TruncateToInt64Low32(RegF[B]);
                break;
            case CastI2S:
                RegS[A] = RegD[B].ToString(CultureInfo.InvariantCulture);
                break;
            case CastU2S:
                RegS[A] = unchecked((uint)RegD[B]).ToString(CultureInfo.InvariantCulture);
                break;
            case CastF2S:
                RegS[A] = FormatFixed5(RegF[B]);
                break;
            case CastV22S:
                RegS[A] = "(" + FormatFixed5(RegF[B]) + ", " + FormatFixed5(RegF[B + 1]) + ")";
                break;
            case CastV32S:
                RegS[A] = "(" + FormatFixed5(RegF[B]) + ", " + FormatFixed5(RegF[B + 1]) + ", " + FormatFixed5(RegF[B + 2]) + ")";
                break;
            case CastV42S:
                RegS[A] = "(" + FormatFixed5(RegF[B]) + ", " + FormatFixed5(RegF[B + 1]) + ", " + FormatFixed5(RegF[B + 2]) + ", " + FormatFixed5(RegF[B + 3]) + ")";
                break;
            case CastP2S:
                RegS[A] = RegA[B] == 0 ? "null" : unchecked((ulong)RegA[B]).ToString("x16", CultureInfo.InvariantCulture);
                break;
            case CastS2I:
                RegD[A] = unchecked((int)ToLongBase0(SourceString(B)));
                break;
            case CastS2F:
                RegF[A] = ToDouble(SourceString(B));
                break;
            case CastS2N:
                string name = SourceString(B);
                RegD[A] = name.Length == 0 ? NameNone : Require(NameIndex, "FName")(name);
                break;
            case CastN2S:
                int index = RegD[B];
                bool valid = (uint)index < (uint)NumNames;
                RegS[A] = valid ? Require(NameChars, "FName")(index) : "";
                break;
            case CastS2Co:
                RegD[A] = Require(ColorOf, "V_GetColor")(SourceString(B));
                break;
            case CastCo2S:
                uint argb = unchecked((uint)RegD[B]);
                int red = (int)((argb >> 16) & 0xff);
                int green = (int)((argb >> 8) & 0xff);
                int blue = (int)(argb & 0xff);
                RegS[A] = red.ToString("x2", CultureInfo.InvariantCulture)
                    + " " + green.ToString("x2", CultureInfo.InvariantCulture)
                    + " " + blue.ToString("x2", CultureInfo.InvariantCulture);
                break;
            case CastS2So:
                RegD[A] = Require(SoundIndex, "S_FindSound")(SourceString(B));
                break;
            case CastSo2S:
                RegS[A] = Require(SoundName, "GetSoundName")(RegD[B]);
                break;
            case CastSid2S:
                uint sprite = unchecked((uint)RegD[B]);
                RegS[A] = SpriteString is null
                    ? unchecked((int)sprite).ToString(CultureInfo.InvariantCulture)
                    : SpriteString(sprite);
                break;
            case CastTid2S:
                string? texture = Require(TextureName, "TexMan")(RegD[B]);
                RegS[A] = texture is null ? "(null)" : texture;
                break;
            default:
                throw new InvalidOperationException("Unknown OP_CAST type\n");
        }
    }

    public void EmitCASTB()
    {
        if (C == CastBInt)
            RegD[A] = RegD[B] != 0 ? 1 : 0;
        else if (C == CastBFloat)
            RegD[A] = RegF[B] != 0 ? 1 : 0;
        else if (C == CastBPointer)
            RegD[A] = RegA[B] != 0 ? 1 : 0;
        else
            RegD[A] = SourceString(B).Length > 0 ? 1 : 0;
    }

    public void EmitDYNCAST_R()
    {
        DynCast(RegA[C], IsKindOf, "IsKindOf");
    }

    public void EmitDYNCAST_K()
    {
        DynCast(KonstA[C], IsKindOf, "IsKindOf");
    }

    public void EmitDYNCASTC_R()
    {
        DynCast(RegA[C], IsDescendantOf, "IsDescendantOf");
    }

    public void EmitDYNCASTC_K()
    {
        DynCast(KonstA[C], IsDescendantOf, "IsDescendantOf");
    }

    void CopyFloats(int count)
    {
        for (int i = 0; i < count; i++)
            RegF[A + i] = RegF[B + i];
    }

    void DynCast(long cls, Func<long, long, bool>? test, string name)
    {
        long obj = RegA[B];
        if (obj == 0)
        {
            RegA[A] = 0;
            return;
        }
        RegA[A] = Require(test, name)(obj, cls) ? obj : 0;
    }

    string SourceString(int index)
    {
        return RegS[index] ?? throw new NullReferenceException("FString");
    }

    static T Require<T>(T? value, string name) where T : class
    {
        return value ?? throw new InvalidOperationException(name);
    }

    public static int TruncateToInt32(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value >= 2147483648.0 || value <= -2147483649.0)
            return int.MinValue;
        return (int)value;
    }

    public static int TruncateToInt64Low32(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value >= 9223372036854775808.0 || value < -9223372036854775808.0)
            return 0;
        return unchecked((int)(long)value);
    }

    public static string FormatFixed5(double value)
    {
        if (double.IsNaN(value))
            return "NaN";
        if (double.IsPositiveInfinity(value))
            return "Inf";
        if (double.IsNegativeInfinity(value))
            return "-Inf";
        bool negative = double.IsNegative(value);
        decimal scaled = (decimal)Math.Abs(value) * 100000m;
        decimal units = decimal.Round(scaled, 0, MidpointRounding.AwayFromZero);
        decimal whole = decimal.Truncate(units / 100000m);
        int frac = (int)(units - (whole * 100000m));
        string body = whole.ToString(CultureInfo.InvariantCulture) + "." + frac.ToString("D5", CultureInfo.InvariantCulture);
        return negative ? "-" + body : body;
    }

    public static long ToLongBase0(string text)
    {
        int i = 0;
        while (i < text.Length && IsCSpace(text[i]))
            i++;
        int sign = 1;
        if (i < text.Length && (text[i] == '+' || text[i] == '-'))
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        int radix = 10;
        if (i < text.Length && text[i] == '0')
        {
            if (i + 2 < text.Length && (text[i + 1] == 'x' || text[i + 1] == 'X') && HexDigit(text[i + 2]) >= 0)
            {
                radix = 16;
                i += 2;
            }
            else
            {
                radix = 8;
            }
        }
        long value = 0;
        bool overflow = false;
        while (i < text.Length)
        {
            int dig = radix == 16 ? HexDigit(text[i]) : text[i] - '0';
            if (dig < 0 || dig >= radix)
                break;
            if (value > (long.MaxValue - dig) / radix)
                overflow = true;
            else
                value = (value * radix) + dig;
            i++;
        }
        if (overflow)
            return sign < 0 ? long.MinValue : long.MaxValue;
        if (sign < 0)
        {
            if (value == long.MinValue)
                return long.MinValue;
            return -value;
        }
        return value;
    }

    public static double ToDouble(string text)
    {
        int i = 0;
        while (i < text.Length && IsCSpace(text[i]))
            i++;
        if (i >= text.Length)
            return 0;
        int sign = 1;
        if (text[i] == '+' || text[i] == '-')
        {
            if (text[i] == '-')
                sign = -1;
            i++;
        }
        if (StartsWithWord(text, i, "nan"))
            return double.NaN;
        if (StartsWithWord(text, i, "infinity") || StartsWithWord(text, i, "inf"))
            return sign < 0 ? double.NegativeInfinity : double.PositiveInfinity;
        if (i + 1 < text.Length && text[i] == '0' && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            return sign * HexFloat(text, i + 2);
        return sign * DecimalFloat(text, i);
    }

    static double HexFloat(string text, int i)
    {
        long digits = 0;
        int count = 0;
        bool any = false;
        while (i < text.Length && HexDigit(text[i]) >= 0 && count < 15)
        {
            digits = (digits << 4) + HexDigit(text[i]);
            count++;
            any = true;
            i++;
        }
        int frac = 0;
        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && HexDigit(text[i]) >= 0 && count < 15)
            {
                digits = (digits << 4) + HexDigit(text[i]);
                count++;
                frac++;
                any = true;
                i++;
            }
        }
        if (!any)
            return 0;
        double value = digits;
        value = Math.ScaleB(value, -4 * frac);
        if (i < text.Length && (text[i] == 'p' || text[i] == 'P'))
        {
            i++;
            int expSign = 1;
            if (i < text.Length && (text[i] == '+' || text[i] == '-'))
            {
                if (text[i] == '-')
                    expSign = -1;
                i++;
            }
            int exp = 0;
            while (i < text.Length && text[i] >= '0' && text[i] <= '9')
            {
                exp = (exp * 10) + (text[i] - '0');
                i++;
            }
            value = Math.ScaleB(value, expSign * exp);
        }
        return value;
    }

    static double DecimalFloat(string text, int i)
    {
        int start = i;
        while (i < text.Length && text[i] >= '0' && text[i] <= '9')
            i++;
        if (i < text.Length && text[i] == '.')
        {
            i++;
            while (i < text.Length && text[i] >= '0' && text[i] <= '9')
                i++;
        }
        if (i < text.Length && (text[i] == 'e' || text[i] == 'E'))
        {
            int e = i + 1;
            if (e < text.Length && (text[e] == '+' || text[e] == '-'))
                e++;
            int expStart = e;
            while (e < text.Length && text[e] >= '0' && text[e] <= '9')
                e++;
            if (e > expStart)
                i = e;
        }
        if (i == start)
            return 0;
        string token = text[start..i];
        if (token == "." || token == "+" || token == "-")
            return 0;
        return double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    static bool StartsWithWord(string text, int i, string word)
    {
        if (i + word.Length > text.Length)
            return false;
        return text.Substring(i, word.Length).Equals(word, StringComparison.OrdinalIgnoreCase);
    }

    static bool IsCSpace(char c)
    {
        return c is ' ' or '\t' or '\n' or '\v' or '\f' or '\r';
    }

    static int HexDigit(char c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'a' && c <= 'f')
            return c - 'a' + 10;
        if (c >= 'A' && c <= 'F')
            return c - 'A' + 10;
        return -1;
    }
}
