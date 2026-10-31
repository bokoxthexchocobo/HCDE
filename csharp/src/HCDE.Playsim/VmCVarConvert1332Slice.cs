using System.Globalization;

namespace HCDE.Playsim;

/// <summary>
/// CVar value conversion, string parse, and the callback gate from
/// <c>c_cvars.cpp</c> (321-676). Chunk 1332. <c>SetGenericRep</c>,
/// <c>ForceSet</c>, and <c>FindCVar</c> stay callees.
/// </summary>
public sealed class VmCVarConvert1332
{
    public const int Bool = 0;
    public const int Int = 1;
    public const int Float = 2;
    public const int String = 3;
    public const int Color = 4;
    public const int Flag = 5;
    public const int Mask = 6;
    public const int Dummy = 7;
    public const int NoInitCall = 1 << 9;
    public const int ZsCustom = 1 << 19;
    public const int StringCap = 39;

    public const string TrueText = "true";
    public const string FalseText = "false";

    public bool DoNoSet;
    public bool UseCallback;
    public string Buffer = "";

    public readonly List<Var> Map = new();
    public readonly List<string> CallbackOrder = new();
    public readonly List<string> ZsInits = new();
    public readonly List<string> ZsMarks = new();
    public readonly List<string> Markers = new();

    public struct Value
    {
        public bool Bool;
        public int Int;
        public float Float;
        public string? Text;
    }

    public sealed class Var
    {
        public string Name = "";
        public int Flags;
        public bool UserCallback;
        public int UserFires;
        public readonly List<string?> GenericSets = new();
        public readonly List<string?> ForceSets = new();
    }

    public Var Add(string name, int flags, bool userCallback)
    {
        if (name.Length == 0)
            throw new InvalidOperationException("cvar name is empty");
        if (Find(name) != null)
            throw new InvalidOperationException("cvar already registered");
        Var var = new Var
        {
            Name = name,
            Flags = flags,
            UserCallback = userCallback,
        };
        Map.Add(var);
        return var;
    }

    public bool ToBool(Value value, int type)
    {
        switch (type)
        {
            case Bool:
                return value.Bool;
            case Color:
            case Int:
                return value.Int != 0;
            case Float:
                return value.Float != 0f;
            case String:
                return BoolFromText(RequireText(value.Text));
            default:
                return false;
        }
    }

    public int ToInt(Value value, int type)
    {
        int res;
        switch (type)
        {
            case Bool:
                res = value.Bool ? 1 : 0;
                break;
            case Color:
            case Int:
                res = value.Int;
                break;
            case Float:
                res = TruncateFloat(value.Float);
                break;
            case String:
                res = IntFromText(RequireText(value.Text));
                break;
            default:
                res = 0;
                break;
        }
        return res;
    }

    public float ToFloat(Value value, int type)
    {
        switch (type)
        {
            case Bool:
                return value.Bool ? 1f : 0f;
            case Color:
            case Int:
                return (float)value.Int;
            case Float:
                return value.Float;
            case String:
                return (float)ParseCDouble(RequireText(value.Text));
            default:
                return 0f;
        }
    }

    public string? ToStringRep(Value value, int type)
    {
        switch (type)
        {
            case Bool:
                return value.Bool ? TrueText : FalseText;
            case String:
                return value.Text;
            case Color:
            case Int:
                WriteBuffer(value.Int.ToString(CultureInfo.InvariantCulture));
                return Buffer;
            case Float:
                WriteBuffer(FormatG(value.Float));
                return Buffer;
            default:
                WriteBuffer("<huh?>");
                return Buffer;
        }
    }

    public Value FromBool(bool value, int type)
    {
        Value ret = default;
        switch (type)
        {
            case Bool:
                ret.Bool = value;
                break;
            case Int:
                ret.Int = value ? 1 : 0;
                break;
            case Float:
                ret.Float = value ? 1f : 0f;
                break;
            case String:
                ret.Text = value ? TrueText : FalseText;
                break;
            default:
                ret.Int = 0;
                break;
        }
        return ret;
    }

    public Value FromInt(int value, int type)
    {
        Value ret = default;
        switch (type)
        {
            case Bool:
                ret.Bool = value != 0;
                break;
            case Int:
                ret.Int = value;
                break;
            case Float:
                ret.Float = (float)value;
                break;
            case String:
                WriteBuffer(value.ToString(CultureInfo.InvariantCulture));
                ret.Text = Buffer;
                break;
            default:
                ret.Int = 0;
                break;
        }
        return ret;
    }

    public Value FromFloat(float value, int type)
    {
        Value ret = default;
        switch (type)
        {
            case Bool:
                ret.Bool = value != 0f;
                break;
            case Int:
                ret.Int = TruncateFloat(value);
                break;
            case Float:
                ret.Float = value;
                break;
            case String:
                WriteBuffer(FormatG(value));
                ret.Text = Buffer;
                break;
            default:
                ret.Int = 0;
                break;
        }
        return ret;
    }

    public Value FromString(string? value, int type)
    {
        string text = RequireText(value);
        Value ret = default;
        switch (type)
        {
            case Bool:
                ret.Bool = BoolFromText(text);
                break;
            case Int:
                ret.Int = IntFromText(text);
                break;
            case Float:
                ret.Float = (float)ParseCDouble(text);
                break;
            case String:
                ret.Text = text;
                break;
            default:
                ret.Int = 0;
                break;
        }
        return ret;
    }

    /// <summary>
    /// Native <c>HexToByte</c> adds the letter offset and does not add 10.
    /// </summary>
    public static byte HexToByte(string hex)
    {
        if (hex.Length < 2)
            throw new InvalidOperationException("hex pair is short");
        int v = 0;
        for (int i = 0; i < 2; i++)
        {
            v <<= 4;
            char c = hex[i];
            if (c >= '0' && c <= '9')
                v += c - '0';
            else if (c >= 'A' && c <= 'F')
                v += c - 'A';
            else
                v += c - 'a';
        }
        return (byte)v;
    }

    public Var? CvarSet(string? name, string? val)
    {
        Var? var = Find(name);
        if (var != null)
            var.GenericSets.Add(val);
        return var;
    }

    public Var? CvarForceSet(string? name, string? val)
    {
        Var? var = Find(name);
        if (var != null)
            var.ForceSets.Add(val);
        return var;
    }

    public void EnableNoSet()
    {
        DoNoSet = true;
    }

    public void EnableCallbacks()
    {
        UseCallback = true;
        for (int i = 0; i < Map.Count; i++)
        {
            Var cvar = Map[i];
            if ((cvar.Flags & NoInitCall) != 0)
                continue;
            CallbackOrder.Add(cvar.Name);
            if (cvar.UserCallback)
                cvar.UserFires++;
        }
    }

    public void InitZSCallbacks()
    {
        for (int i = 0; i < Map.Count; i++)
        {
            Var cvar = Map[i];
            if ((cvar.Flags & ZsCustom) == 0)
                continue;
            ZsInits.Add(cvar.Name);
        }
        Markers.Add("MarkZSCallbacks");
    }

    public void MarkZSCallbacks()
    {
        for (int i = 0; i < Map.Count; i++)
        {
            Var cvar = Map[i];
            if ((cvar.Flags & ZsCustom) == 0)
                continue;
            ZsMarks.Add(cvar.Name);
        }
    }

    public void DisableCallbacks()
    {
        UseCallback = false;
    }

    public Var? Find(string? name)
    {
        if (string.IsNullOrEmpty(name))
            return null;
        for (int i = 0; i < Map.Count; i++)
        {
            if (string.Equals(Map[i].Name, name, StringComparison.OrdinalIgnoreCase))
                return Map[i];
        }
        return null;
    }

    public static string FormatG(float value)
    {
        if (float.IsNaN(value))
        {
            if (BitConverter.SingleToInt32Bits(value) < 0)
                return "-nan";
            return "nan";
        }
        if (float.IsPositiveInfinity(value))
            return "inf";
        if (float.IsNegativeInfinity(value))
            return "-inf";
        if (value == 0f)
        {
            if (BitConverter.SingleToInt32Bits(value) < 0)
                return "-0";
            return "0";
        }
        return ((double)value).ToString("G6", CultureInfo.InvariantCulture).Replace('E', 'e');
    }

    void WriteBuffer(string text)
    {
        if (text.Length > StringCap)
            text = text.Substring(0, StringCap);
        Buffer = text;
    }

    static string RequireText(string? text)
    {
        if (text == null)
            throw new InvalidOperationException("null string");
        return text;
    }

    static bool BoolFromText(string text)
    {
        if (text.Equals(TrueText, StringComparison.OrdinalIgnoreCase))
            return true;
        if (text.Equals(FalseText, StringComparison.OrdinalIgnoreCase))
            return false;
        return ParseCLong(text) != 0;
    }

    static int IntFromText(string text)
    {
        if (text.Equals(TrueText, StringComparison.OrdinalIgnoreCase))
            return 1;
        if (text.Equals(FalseText, StringComparison.OrdinalIgnoreCase))
            return 0;
        return unchecked((int)ParseCLong(text));
    }

    static int TruncateFloat(float value)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
            throw new InvalidOperationException("float is not an int");
        if (value >= 2147483648f || value < -2147483648f)
            throw new InvalidOperationException("float is outside int");
        return (int)value;
    }

    static bool IsCSpace(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
    }

    static int Digit(char c, int radix)
    {
        int d;
        if (c >= '0' && c <= '9')
            d = c - '0';
        else if (c >= 'a' && c <= 'z')
            d = c - 'a' + 10;
        else if (c >= 'A' && c <= 'Z')
            d = c - 'A' + 10;
        else
            return -1;
        if (d >= radix)
            return -1;
        return d;
    }

    static int HexDigit(char c)
    {
        return Digit(c, 16);
    }

    static bool StartsAt(string text, int index, string word)
    {
        if (index + word.Length > text.Length)
            return false;
        return text.AsSpan(index, word.Length).Equals(word, StringComparison.OrdinalIgnoreCase);
    }

    public static long ParseCLong(string text)
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
            if (i + 1 < n && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            {
                radix = 16;
                i += 2;
            }
            else
            {
                radix = 8;
            }
        }
        bool any = false;
        bool overflow = false;
        ulong mag = 0;
        while (i < n)
        {
            int d = Digit(text[i], radix);
            if (d < 0)
                break;
            any = true;
            if (overflow || mag > (ulong.MaxValue - (ulong)d) / (ulong)radix)
                overflow = true;
            else
                mag = mag * (ulong)radix + (ulong)d;
            i++;
        }
        if (!any)
            return 0;
        if (overflow)
            return sign < 0 ? long.MinValue : long.MaxValue;
        if (sign < 0)
        {
            if (mag >= (ulong)long.MaxValue + 1UL)
                return long.MinValue;
            return -(long)mag;
        }
        if (mag > (ulong)long.MaxValue)
            return long.MaxValue;
        return (long)mag;
    }

    public static double ParseCDouble(string text)
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
        if (StartsAt(text, i, "infinity") || StartsAt(text, i, "inf"))
            return sign < 0 ? double.NegativeInfinity : double.PositiveInfinity;
        if (StartsAt(text, i, "nan"))
        {
            long bits = BitConverter.DoubleToInt64Bits(double.NaN);
            if (sign < 0)
                bits |= long.MinValue;
            return BitConverter.Int64BitsToDouble(bits);
        }
        if (i + 1 < n && text[i] == '0' && (text[i + 1] == 'x' || text[i + 1] == 'X'))
            return ParseHexFloat(text, i + 2, sign);
        if (!TryParseDecimal(text, i, out double value))
            return 0d;
        if (sign < 0)
            return -value;
        return value;
    }

    static double ParseHexFloat(string text, int i, int sign)
    {
        int n = text.Length;
        double mag = 0;
        bool any = false;
        while (i < n)
        {
            int d = HexDigit(text[i]);
            if (d < 0)
                break;
            any = true;
            mag = mag * 16 + d;
            i++;
        }
        if (i < n && text[i] == '.')
        {
            i++;
            double scale = 1;
            while (i < n)
            {
                int d = HexDigit(text[i]);
                if (d < 0)
                    break;
                any = true;
                scale /= 16d;
                mag += d * scale;
                i++;
            }
        }
        if (!any)
            return 0d;
        if (i < n && (text[i] == 'p' || text[i] == 'P'))
        {
            int j = i + 1;
            int expSign = 1;
            if (j < n && (text[j] == '+' || text[j] == '-'))
            {
                if (text[j] == '-')
                    expSign = -1;
                j++;
            }
            bool expDigits = false;
            long exp = 0;
            while (j < n && text[j] >= '0' && text[j] <= '9')
            {
                expDigits = true;
                exp = exp * 10 + (text[j] - '0');
                if (exp > 100000)
                    break;
                j++;
            }
            if (expDigits)
            {
                if (exp > 100000)
                    mag = expSign < 0 ? 0d : double.PositiveInfinity;
                else
                    mag = Math.ScaleB(mag, expSign * (int)exp);
            }
        }
        if (sign < 0)
            mag = -mag;
        return mag;
    }

    static bool TryParseDecimal(string text, int i, out double value)
    {
        int n = text.Length;
        int start = i;
        while (i < n && text[i] >= '0' && text[i] <= '9')
            i++;
        if (i < n && text[i] == '.')
        {
            i++;
            while (i < n && text[i] >= '0' && text[i] <= '9')
                i++;
        }
        if (!HasDigit(text, start, i))
        {
            value = 0d;
            return false;
        }
        int end = i;
        if (i < n && (text[i] == 'e' || text[i] == 'E'))
        {
            int j = i + 1;
            if (j < n && (text[j] == '+' || text[j] == '-'))
                j++;
            int expStart = j;
            while (j < n && text[j] >= '0' && text[j] <= '9')
                j++;
            if (j > expStart)
                end = j;
        }
        string slice = text.Substring(start, end - start);
        if (!double.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            value = 0d;
            return false;
        }
        return true;
    }

    static bool HasDigit(string text, int start, int end)
    {
        for (int i = start; i < end; i++)
        {
            if (text[i] >= '0' && text[i] <= '9')
                return true;
        }
        return false;
    }
}
