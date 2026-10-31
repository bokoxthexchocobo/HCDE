namespace HCDE.Playsim;

/// <summary>
/// XLAT keyword search and translator load from <c>parse_xlat.cpp</c> (<b>1–167</b>).
/// Chunk <b>1217</b>. Keyword compare is ASCII case-insensitive. <c>lineid</c> stores the same code as <c>tag</c>.
/// A null lump name is <c>NAME_None</c> (<c>None</c>) and does not share the empty-string translator.
/// </summary>
public sealed class VmXlatTranslator1217
{
    public bool Loaded;
    public int Identity;
}

public static class VmXlat1217Slice
{
    public const int Include = 128;
    public const int Tag = 21;

    static readonly string[] Tokens =
    {
        "arg2", "arg3", "arg4", "arg5", "bitmask", "clear",
        "define", "enum", "flags", "include", "lineflag", "lineid",
        "maxlinespecial", "nobitmask", "sector", "tag",
    };

    static readonly short[] Types =
    {
        25, 26, 27, 28, 35, 36,
        14, 17, 24, 128, 39, 21,
        31, 34, 33, 21,
    };

    static int nextIdentity = 1;
    static readonly Dictionary<string, VmXlatTranslator1217> loaded = new(StringComparer.Ordinal);
    static VmXlatTranslator1217? none;

    internal static void Reset()
    {
        loaded.Clear();
        none = null;
        nextIdentity = 1;
    }

    public static bool FindToken(string? token, ref int type)
    {
        if (token == null)
            throw new NullReferenceException();
        int min = 0;
        int max = Tokens.Length - 1;
        while (min <= max)
        {
            int mid = (min + max) / 2;
            int lex = Stricmp(token, Tokens[mid]);
            if (lex == 0)
            {
                type = Types[mid];
                return true;
            }
            if (lex > 0)
                min = mid + 1;
            else
                max = mid - 1;
        }
        return false;
    }

    public static VmXlatTranslator1217 Load(
        string? lumpName,
        Func<int> alloc,
        Action<int, string?> parseLump,
        Action<int> parseZero,
        Action<int> free)
    {
        VmXlatTranslator1217 translator = TranslatorFor(lumpName);
        if (!translator.Loaded)
        {
            int parser = alloc();
            parseLump(parser, lumpName);
            parseZero(parser);
            free(parser);
            translator.Loaded = true;
        }
        return translator;
    }

    static VmXlatTranslator1217 TranslatorFor(string? lumpName)
    {
        if (lumpName == null || Lower(lumpName) == "none")
        {
            none ??= Create();
            return none;
        }
        string key = Lower(lumpName);
        if (!loaded.TryGetValue(key, out VmXlatTranslator1217? translator))
        {
            translator = Create();
            loaded[key] = translator;
        }
        return translator;
    }

    static VmXlatTranslator1217 Create()
    {
        return new VmXlatTranslator1217 { Identity = nextIdentity++ };
    }

    static string Lower(string value)
    {
        char[] chars = new char[value.Length];
        for (int i = 0; i < value.Length; i++)
        {
            char ch = value[i];
            if (ch >= 'A' && ch <= 'Z')
                ch = (char)(ch + 32);
            chars[i] = ch;
        }
        return new string(chars);
    }

    static int Stricmp(string left, string right)
    {
        int n = left.Length < right.Length ? left.Length : right.Length;
        for (int i = 0; i < n; i++)
        {
            int a = Fold(left[i]);
            int b = Fold(right[i]);
            if (a != b)
                return a - b;
        }
        return left.Length - right.Length;
    }

    static int Fold(char ch)
    {
        if (ch >= 'A' && ch <= 'Z')
            return ch + 32;
        return ch;
    }
}
