namespace HCDE.Client;

/// <summary><c>ParseCVarInfo</c> and <c>ConsiderPatches</c> from <c>d_main.cpp</c> (2086–2267).</summary>
public static class VmDMainParseCVarInfo
{
    public const int CvarModArchive = 1;
    public const int CvarServerInfo = 2;
    public const int CvarUserInfo = 4;
    public const int CvarCheat = 8;
    public const int CvarLatch = 16;
    public const int CvarConfigOnly = 32;

    public static int ApplyAttributeFlag(string token, int flags, out bool handlerClass, out string? handlerClassName)
    {
        handlerClass = false;
        handlerClassName = null;
        if (token.Equals("server", StringComparison.OrdinalIgnoreCase))
            return flags | CvarServerInfo;
        if (token.Equals("user", StringComparison.OrdinalIgnoreCase))
            return flags | CvarUserInfo;
        if (token.Equals("noarchive", StringComparison.OrdinalIgnoreCase))
            return flags & ~CvarModArchive;
        if (token.Equals("cheat", StringComparison.OrdinalIgnoreCase))
            return flags | CvarCheat;
        if (token.Equals("latch", StringComparison.OrdinalIgnoreCase))
            return flags | CvarLatch;
        if (token.Equals("nosave", StringComparison.OrdinalIgnoreCase))
            return flags | CvarConfigOnly;
        if (token.Equals("handlerClass", StringComparison.OrdinalIgnoreCase))
        {
            handlerClass = true;
            return flags;
        }

        throw new InvalidOperationException($"Unknown cvar attribute '{token}'");
    }

    public static int NormalizeConfigOnlyFlags(int flags)
    {
        if ((flags & CvarConfigOnly) == 0)
            return flags;
        return flags & ~(CvarServerInfo | CvarUserInfo);
    }

    public static bool HasValidScopeFlags(int flags)
    {
        var scope = flags & (CvarServerInfo | CvarUserInfo | CvarConfigOnly);
        if (scope == 0)
            return false;
        if ((flags & (CvarServerInfo | CvarUserInfo)) == (CvarServerInfo | CvarUserInfo))
            return false;
        return true;
    }

    public static string? MapTypeToken(string token) =>
        token switch
        {
            "bool" => "bool",
            "int" => "int",
            "float" => "float",
            "color" => "color",
            "string" => "string",
            _ => null
        };

    public static bool IsBoolDefaultToken(string token) =>
        token.Equals("true", StringComparison.OrdinalIgnoreCase)
        || token.Equals("false", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldRunModConfigSetup(bool addedCvars) => addedCvars;

    public static bool ConsiderPatchesFoundAny(int argc) => argc > 0;

    public static bool IsPatchExtension(string path) =>
        path.EndsWith(".deh", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".bex", StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<string> EnumeratePatchCandidates(IReadOnlyList<string> args)
    {
        foreach (var arg in args)
        {
            if (IsPatchExtension(arg))
                yield return arg;
        }
    }
}
