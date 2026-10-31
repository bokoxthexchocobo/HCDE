using System.Globalization;
using System.Numerics;

namespace HCDE.Playsim;

/// <summary>
/// Lexer and include-aware lump walk from <c>parsecontext.cpp</c> (<b>1–340</b>).
/// Chunk <b>1221</b>. Token codes come from the caller table. A single <c>&lt;</c> or <c>&gt;</c>
/// consumes the next character and returns 0. A 79-character identifier whose last stored
/// character is <c>_</c> is ungot. <c>strtoll</c> then the int cast keeps the low 32 bits.
/// </summary>
public sealed class VmParseContext1221
{
    public const int Or = 1;
    public const int Xor = 2;
    public const int And = 3;
    public const int Minus = 4;
    public const int Plus = 5;
    public const int Multiply = 6;
    public const int Divide = 7;
    public const int Modulus = 8;
    public const int Num = 9;
    public const int FloatVal = 10;
    public const int LParen = 11;
    public const int RParen = 12;
    public const int Sym = 13;
    public const int RBrace = 14;
    public const int LBrace = 15;
    public const int Comma = 16;
    public const int EqualsToken = 17;
    public const int LBracket = 18;
    public const int RBracket = 19;
    public const int OrEqual = 20;
    public const int Colon = 21;
    public const int Semicolon = 22;
    public const int LshAssign = 23;
    public const int RshAssign = 24;
    public const int StringToken = 25;
    public const int Include = 26;
    public const int Define = 27;

    public int SourceLine;
    public int EnumVal;
    public readonly int[] TokenTrans = new int[32];
    public readonly List<VmParseSymbol1221> Symbols = new();
    public readonly List<string> Prints = new();
    public VmFindToken1221? FindToken;
    public Func<string, int>? FindLineSpecial;
    public Func<string, bool, int>? CheckNumForFullName;
    public Func<int, string>? GetStringFromLump;
    public Action<int, VmParseToken1221>? OnParse;

    VmParseName1221? sourceName;

    public VmParseContext1221()
    {
        for (int i = 0; i < TokenTrans.Length; i++)
            TokenTrans[i] = i;
    }

    public string? SourceFile
    {
        get
        {
            if (sourceName == null)
                return null;
            return sourceName.Text;
        }
    }

    public void SetSourceFile(string? file)
    {
        if (file == null)
            sourceName = null;
        else
            sourceName = VmParseName1221.Stable(file);
    }

    public void AddSym(string sym, int val)
    {
        var stored = new VmParseSymbol1221();
        stored.Value = val;
        int n = sym.Length;
        if (n > 79)
            n = 79;
        stored.Name = sym.Substring(0, n);
        Symbols.Add(stored);
    }

    public bool FindSym(string sym, out VmParseSymbol1221? found)
    {
        for (int i = 0; i < Symbols.Count; i++)
        {
            if (Symbols[i].Name == sym)
            {
                found = Symbols[i];
                return true;
            }
        }
        found = null;
        return false;
    }

    public int GetToken(VmParseCursor1221 source, VmParseToken1221 yylval)
    {
        int c;
        var token = new char[80];
        int toksize;
    loop:
        while (IsSpace(c = source.Read()) && c != 0)
        {
            if (c == '\n')
                SourceLine++;
        }
        if (c == 0)
            return 0;
        if (IsDigit(c))
        {
            if (c == '0')
            {
                c = source.Read();
                if (c == 'x' || c == 'X')
                {
                    long hex = StrToLl(source.Text, source.Pos, 16, out int hexEnd);
                    source.Pos = hexEnd;
                    yylval.Val = unchecked((int)hex);
                    return TokenTrans[Num];
                }
                source.Unget();
            }
            source.Unget();
            int numStart = source.Pos;
            long dec = StrToLl(source.Text, numStart, 10, out int endp);
            if (At(source.Text, endp) == '.')
            {
                yylval.FVal = StrToD(source.Text, numStart, out int floatEnd);
                source.Pos = floatEnd;
                return TokenTrans[FloatVal];
            }
            source.Pos = endp;
            yylval.Val = unchecked((int)dec);
            return TokenTrans[Num];
        }
        if (IsAlpha(c))
        {
            int buildup = 0;
            token[0] = (char)c;
            toksize = 1;
            while (toksize < 79 && (IsAlnum(c = source.Read()) || c == '_'))
                token[toksize++] = (char)c;
            token[toksize] = '\0';
            if (toksize == 79 && IsAlnum(c))
            {
                while (IsAlnum(c = source.Read()))
                    token[79] = (char)c;
            }
            source.Unget();
            string name = new string(token, 0, toksize);
            if (FindToken != null && FindToken(name, out buildup))
                return buildup;
            if (FindSym(name, out VmParseSymbol1221? found) && found != null)
            {
                yylval.Val = found.Value;
                return TokenTrans[Num];
            }
            int special = FindLineSpecial != null ? FindLineSpecial(name) : 0;
            yylval.Val = special;
            if (special != 0)
                return TokenTrans[Num];
            yylval.SetCString(name);
            return TokenTrans[Sym];
        }
        if (c == '/')
        {
            c = source.Read();
            if (c == '*')
            {
                for (;;)
                {
                    while ((c = source.Read()) != '*' && c != 0)
                    {
                        if (c == '\n')
                            SourceLine++;
                    }
                    if (c == 0)
                        return 0;
                    if ((c = source.Read()) == '/')
                        goto loop;
                    if (c == 0)
                        return 0;
                    source.Unget();
                }
            }
            if (c == '/')
            {
                while ((c = source.Read()) != '\n' && c != 0)
                {
                }
                if (c == '\n')
                    SourceLine++;
                else if (c == -1)
                    return 0;
                goto loop;
            }
            source.Unget();
            return TokenTrans[Divide];
        }
        if (c == '"')
        {
            int tokensize = 0;
            while ((c = source.Read()) != '"' && c != 0)
            {
                yylval.PutStringByte(tokensize, c);
                tokensize++;
            }
            yylval.PutStringByte(tokensize, 0);
            return TokenTrans[StringToken];
        }
        if (c == '|')
        {
            c = source.Read();
            if (c == '=')
                return TokenTrans[OrEqual];
            source.Unget();
            return TokenTrans[Or];
        }
        if (c == '<')
        {
            c = source.Read();
            if (c == '<')
            {
                c = source.Read();
                if (c == '=')
                    return TokenTrans[LshAssign];
                source.Unget();
                return 0;
            }
            c--;
            return 0;
        }
        if (c == '>')
        {
            c = source.Read();
            if (c == '>')
            {
                c = source.Read();
                if (c == '=')
                    return TokenTrans[RshAssign];
                source.Unget();
                return 0;
            }
            c--;
            return 0;
        }
        if (c == '#')
        {
            if (StartsWithFold(source.Text, source.Pos, "include"))
            {
                source.Pos += 7;
                return TokenTrans[Include];
            }
            if (StartsWithFold(source.Text, source.Pos, "define"))
            {
                source.Pos += 6;
                return TokenTrans[Define];
            }
        }
        switch (c)
        {
            case '^': return TokenTrans[Xor];
            case '&': return TokenTrans[And];
            case '-': return TokenTrans[Minus];
            case '+': return TokenTrans[Plus];
            case '*': return TokenTrans[Multiply];
            case '%': return TokenTrans[Modulus];
            case '(': return TokenTrans[LParen];
            case ')': return TokenTrans[RParen];
            case ',': return TokenTrans[Comma];
            case '{': return TokenTrans[LBrace];
            case '}': return TokenTrans[RBrace];
            case '=': return TokenTrans[EqualsToken];
            case ';': return TokenTrans[Semicolon];
            case ':': return TokenTrans[Colon];
            case '[': return TokenTrans[LBracket];
            case ']': return TokenTrans[RBracket];
            default: return 0;
        }
    }

    public int PrintError(string message)
    {
        if (sourceName != null)
            Prints.Add(sourceName.Text + ", line " + SourceLine.ToString(CultureInfo.InvariantCulture) + ": " + message + "\n");
        else
            Prints.Add(message + "\n");
        return 0;
    }

    public void ParseLump(string lumpname)
    {
        ParseLumpCore(VmParseName1221.Stable(lumpname));
    }

    void ParseLumpCore(VmParseName1221 lumpname)
    {
        int savedLine = SourceLine;
        VmParseName1221? savedFile = sourceName;
        var token = new VmParseToken1221();
        int lumpno = CheckNumForFullName!(lumpname.Text, true);
        if (lumpno == -1)
        {
            string shown = sourceName == null ? "(null)" : sourceName.Text;
            Prints.Add(shown + ", line " + SourceLine.ToString(CultureInfo.InvariantCulture) + ": Lump '" + lumpname.Text + "' not found\n");
            return;
        }
        SourceLine = 0;
        sourceName = lumpname;
        string body = GetStringFromLump!(lumpno);
        var source = new VmParseCursor1221(body);
        int tokentype;
        while ((tokentype = GetToken(source, token)) != 0)
        {
            if (tokentype == TokenTrans[Include])
            {
                if (GetToken(source, token) != TokenTrans[StringToken])
                {
                    Prints.Add(sourceName.Text + ", line " + SourceLine.ToString(CultureInfo.InvariantCulture) + ": Include: String parameter expected\n");
                    return;
                }
                ParseLumpCore(VmParseName1221.Alias(token.Raw));
            }
            else
            {
                OnParse!(tokentype, token);
            }
        }
        SourceLine = savedLine;
        sourceName = savedFile;
    }

    static int At(string text, int index)
    {
        if (index < 0 || index >= text.Length)
            return 0;
        return text[index];
    }

    static bool IsSpace(int c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '\v' || c == '\f';
    }

    static bool IsDigit(int c)
    {
        return c >= '0' && c <= '9';
    }

    static bool IsAlpha(int c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    static bool IsAlnum(int c)
    {
        return IsDigit(c) || IsAlpha(c);
    }

    static int Fold(int c)
    {
        if (c >= 'A' && c <= 'Z')
            return c + 32;
        return c;
    }

    static bool StartsWithFold(string text, int pos, string lit)
    {
        for (int i = 0; i < lit.Length; i++)
        {
            int a = At(text, pos + i);
            int b = lit[i];
            if (Fold(a) != Fold(b))
                return false;
            if (a == 0)
                return false;
        }
        return true;
    }

    static int HexVal(int c)
    {
        if (c >= '0' && c <= '9')
            return c - '0';
        if (c >= 'a' && c <= 'f')
            return c - 'a' + 10;
        if (c >= 'A' && c <= 'F')
            return c - 'A' + 10;
        return -1;
    }

    static long StrToLl(string text, int start, int radix, out int end)
    {
        int n = start;
        while (IsSpace(At(text, n)))
            n++;
        int neg = 0;
        int sign = At(text, n);
        if (sign == '+' || sign == '-')
        {
            neg = sign == '-' ? 1 : 0;
            n++;
        }
        if (radix == 16 && At(text, n) == '0' && (At(text, n + 1) == 'x' || At(text, n + 1) == 'X') && HexVal(At(text, n + 2)) >= 0)
            n += 2;
        BigInteger acc = 0;
        int any = 0;
        for (;;)
        {
            int d = radix == 16 ? HexVal(At(text, n)) : (IsDigit(At(text, n)) ? At(text, n) - '0' : -1);
            if (d < 0)
                break;
            any = 1;
            acc = acc * radix + d;
            n++;
        }
        if (any == 0)
        {
            end = start;
            return 0;
        }
        end = n;
        if (neg == 0)
        {
            if (acc > long.MaxValue)
                return long.MaxValue;
            return (long)acc;
        }
        BigInteger limit = (BigInteger)long.MaxValue + 1;
        if (acc >= limit)
            return long.MinValue;
        return -(long)acc;
    }

    static double StrToD(string text, int start, out int end)
    {
        int n = start;
        while (IsSpace(At(text, n)))
            n++;
        int numStart = n;
        int sign = At(text, n);
        if (sign == '+' || sign == '-')
            n++;
        int digits = 0;
        while (IsDigit(At(text, n)))
        {
            digits++;
            n++;
        }
        if (At(text, n) == '.')
        {
            n++;
            while (IsDigit(At(text, n)))
            {
                digits++;
                n++;
            }
        }
        if (digits == 0)
        {
            end = start;
            return 0;
        }
        int mark = At(text, n);
        if (mark == 'e' || mark == 'E')
        {
            int e = n + 1;
            int es = At(text, e);
            if (es == '+' || es == '-')
                e++;
            if (IsDigit(At(text, e)))
            {
                n = e;
                while (IsDigit(At(text, n)))
                    n++;
            }
        }
        end = n;
        string slice = text.Substring(numStart, end - numStart);
        return double.Parse(slice, NumberStyles.Float, CultureInfo.InvariantCulture);
    }
}

public delegate bool VmFindToken1221(string token, out int type);

public sealed class VmParseSymbol1221
{
    public int Value;
    public string Name = "";
}

public sealed class VmParseToken1221
{
    public readonly byte[] Raw = new byte[80];

    public int Val
    {
        get { return BitConverter.ToInt32(Raw, 0); }
        set
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Raw[0] = bytes[0];
            Raw[1] = bytes[1];
            Raw[2] = bytes[2];
            Raw[3] = bytes[3];
        }
    }

    public double FVal
    {
        get { return BitConverter.ToDouble(Raw, 0); }
        set
        {
            byte[] bytes = BitConverter.GetBytes(value);
            for (int i = 0; i < 8; i++)
                Raw[i] = bytes[i];
        }
    }

    public string Sym
    {
        get { return ReadCString(Raw); }
    }

    public string String
    {
        get { return ReadCString(Raw); }
    }

    public void SetCString(string text)
    {
        int n = text.Length;
        if (n > 79)
            n = 79;
        for (int i = 0; i < n; i++)
            Raw[i] = (byte)text[i];
        Raw[n] = 0;
    }

    public void PutStringByte(int index, int value)
    {
        Raw[index] = (byte)value;
    }

    public void Clear()
    {
        for (int i = 0; i < Raw.Length; i++)
            Raw[i] = 0;
    }

    static string ReadCString(byte[] raw)
    {
        int n = 0;
        while (n < raw.Length && raw[n] != 0)
            n++;
        var chars = new char[n];
        for (int i = 0; i < n; i++)
            chars[i] = (char)raw[i];
        return new string(chars);
    }
}

public sealed class VmParseCursor1221
{
    public readonly string Text;
    public int Pos;

    public VmParseCursor1221(string text)
    {
        Text = text;
    }

    public int Read()
    {
        int c;
        if (Pos >= Text.Length)
            c = 0;
        else
            c = Text[Pos];
        Pos++;
        return c;
    }

    public void Unget()
    {
        if (Pos > 0)
            Pos--;
    }
}

sealed class VmParseName1221
{
    public string? StableText;
    public byte[]? Buffer;

    public static VmParseName1221 Stable(string text)
    {
        var name = new VmParseName1221();
        name.StableText = text;
        return name;
    }

    public static VmParseName1221 Alias(byte[] raw)
    {
        var name = new VmParseName1221();
        name.Buffer = raw;
        return name;
    }

    public string Text
    {
        get
        {
            if (Buffer == null)
                return StableText ?? "";
            int n = 0;
            while (n < Buffer.Length && Buffer[n] != 0)
                n++;
            var chars = new char[n];
            for (int i = 0; i < n; i++)
                chars[i] = (char)Buffer[i];
            return new string(chars);
        }
    }
}
