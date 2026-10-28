namespace HCDE.Scripting;

/// <summary>Native <c>sc_man.h</c> token constants (<c>TK_SequenceStart</c> = 256).</summary>
public static class VmScriptTokens
{
    public const int SequenceStart = 256;
    public const int Identifier = 256;
    public const int StringConst = 257;
    public const int NameConst = 258;
    public const int IntConst = 259;
    public const int UIntConst = 260;
    public const int FloatConst = 261;
    public const int NonWhitespace = 262;
    public const int Break = 265;
    public const int If = 273;
    public const int True = 311;
    public const int False = 312;

    private static readonly Dictionary<string, int> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        ["break"] = Break,
        ["if"] = If,
        ["true"] = True,
        ["false"] = False,
        ["int"] = 291,
    };

    public static bool TryGetKeyword(string text, out int tokenType)
    {
        if (Keywords.TryGetValue(text, out tokenType)) return true;
        tokenType = Identifier;
        return false;
    }
}
