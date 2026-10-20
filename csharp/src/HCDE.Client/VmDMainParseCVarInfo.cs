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

    public enum CvarInfoType
    {
        Dummy,
        Bool,
        Int,
        Float,
        Color,
        String
    }

    public readonly record struct CvarInfoDefinition(
        string Name,
        CvarInfoType Type,
        int Flags,
        bool CustomHandler,
        string? HandlerClassName,
        string? DefaultLiteral);

    public static int DefaultModFlags => CvarModArchive;

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

    public static int ParseAttributeTokens(
        IReadOnlyList<string> tokens,
        ref int index,
        out bool customHandler,
        out string? handlerClassName)
    {
        var flags = DefaultModFlags;
        customHandler = false;
        handlerClassName = null;
        while (index < tokens.Count && IsAttributeIdentifier(tokens[index]))
        {
            var handlerToken = false;
            flags = ApplyAttributeFlag(tokens[index], flags, out handlerToken, out _);
            index++;
            if (handlerToken)
            {
                if (index >= tokens.Count || tokens[index] != "(")
                    throw new InvalidOperationException("Expected '(' after handlerClass");
                index++;
                if (index >= tokens.Count)
                    throw new InvalidOperationException("Expected handler class name");
                handlerClassName = tokens[index++];
                customHandler = true;
                if (index >= tokens.Count || tokens[index] != ")")
                    throw new InvalidOperationException("Expected ')' after handler class name");
                index++;
            }
        }

        return NormalizeConfigOnlyFlags(flags);
    }

    public static bool IsAttributeIdentifier(string token) =>
        !token.Equals("bool", StringComparison.OrdinalIgnoreCase)
        && !token.Equals("int", StringComparison.OrdinalIgnoreCase)
        && !token.Equals("float", StringComparison.OrdinalIgnoreCase)
        && !token.Equals("color", StringComparison.OrdinalIgnoreCase)
        && !token.Equals("string", StringComparison.OrdinalIgnoreCase)
        && token != ";"
        && token != "=";

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

    public static CvarInfoType ResolveTypeKeyword(string token) =>
        token.ToLowerInvariant() switch
        {
            "bool" => CvarInfoType.Bool,
            "int" => CvarInfoType.Int,
            "float" => CvarInfoType.Float,
            "color" => CvarInfoType.Color,
            "string" => CvarInfoType.String,
            _ => CvarInfoType.Dummy
        };

    public static bool TryParseDefaultValue(
        CvarInfoType type,
        IReadOnlyList<string> tokens,
        ref int index,
        out string? literal,
        out string? error)
    {
        literal = null;
        error = null;
        if (index >= tokens.Count || tokens[index] != "=")
            return true;
        index++;
        if (index >= tokens.Count)
        {
            error = "Expected default value after '='";
            return false;
        }

        switch (type)
        {
            case CvarInfoType.Bool:
                if (!IsBoolDefaultToken(tokens[index]))
                {
                    error = "Expected true or false";
                    return false;
                }

                literal = tokens[index++];
                return true;
            case CvarInfoType.Int:
            case CvarInfoType.Float:
                literal = tokens[index++];
                return true;
            default:
                literal = tokens[index++];
                return true;
        }
    }

    public static bool TryParseDefinition(
        IReadOnlyList<string> tokens,
        ref int index,
        ISet<string> existingNames,
        out CvarInfoDefinition definition,
        out string? error)
    {
        definition = default;
        error = null;
        if (index >= tokens.Count)
            return false;

        var flags = ParseAttributeTokens(tokens, ref index, out var customHandler, out var handlerClass);
        if (!HasValidScopeFlags(flags))
        {
            error = "One of 'server', 'user', or 'nosave' must be specified";
            return false;
        }

        if (index >= tokens.Count)
        {
            error = "Bad cvar type";
            return false;
        }

        var type = ResolveTypeKeyword(tokens[index]);
        if (type == CvarInfoType.Dummy)
        {
            error = $"Bad cvar type '{tokens[index]}'";
            return false;
        }

        index++;
        if (index >= tokens.Count)
        {
            error = "Expected cvar name";
            return false;
        }

        var name = tokens[index++];
        if (existingNames.Contains(name))
        {
            error = $"cvar '{name}' already exists";
            return false;
        }

        if (!TryParseDefaultValue(type, tokens, ref index, out var literal, out error))
            return false;

        if (index >= tokens.Count || tokens[index] != ";")
        {
            error = "Expected ';' after cvar definition";
            return false;
        }

        index++;
        existingNames.Add(name);
        definition = new CvarInfoDefinition(name, type, flags, customHandler, handlerClass, literal);
        return true;
    }

    public static int ParseCvarInfoLump(
        IReadOnlyList<string> tokens,
        ICollection<CvarInfoDefinition> sink,
        ISet<string> existingNames)
    {
        var index = 0;
        var added = 0;
        while (index < tokens.Count)
        {
            if (!TryParseDefinition(tokens, ref index, existingNames, out var def, out _))
                break;
            sink.Add(def);
            added++;
        }

        return added;
    }

    public static bool IsBoolDefaultToken(string token) =>
        token.Equals("true", StringComparison.OrdinalIgnoreCase)
        || token.Equals("false", StringComparison.OrdinalIgnoreCase);

    public static bool ShouldRunModConfigSetup(bool addedCvars) => addedCvars;

    public static bool ConsiderPatchesFoundAny(int argc) => argc > 0;

    public static bool IsPatchExtension(string path) =>
        path.EndsWith(".deh", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".bex", StringComparison.OrdinalIgnoreCase);

    public static List<string> ResolvePatchLoads(
        IReadOnlyList<string> args,
        Func<string, string?> tryResolveDeh,
        Func<string, string?> tryResolveBex)
    {
        var loaded = new List<string>();
        foreach (var arg in args)
        {
            var path = tryResolveDeh(arg) ?? tryResolveBex(arg);
            if (path != null)
                loaded.Add(path);
        }

        return loaded;
    }
}
