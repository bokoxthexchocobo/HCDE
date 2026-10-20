namespace HCDE.Gamedata;

/// <summary><c>GenericParse</c> and field tables from <c>p_terrain.cpp</c>.</summary>
public enum VmTerrainGenericType
{
    End,
    Sound,
    Byte,
    Class,
    Splash,
    Float,
    Double,
    Bool,
    Int,
    Custom,
}

public delegate void VmTerrainGenericCustomHandler(
    IReadOnlyList<string> tokens,
    ref int index,
    VmTerrainGenericContext context,
    List<string> errors);

public sealed class VmTerrainGenericDescriptor
{
    public VmTerrainGenericType Type { get; init; }
    public VmTerrainGenericCustomHandler? Custom { get; init; }
}

public sealed class VmTerrainGenericContext
{
    public VmTerrainDatabase Database { get; init; } = null!;
    public object Target { get; init; } = null!;
    public string OwnerType { get; init; } = "";
    public string OwnerName { get; init; } = "";
}

public static class VmTerrainGenericParser
{
    public static void ParseBody(
        IReadOnlyList<string> tokens,
        ref int i,
        IReadOnlyList<string> keywords,
        IReadOnlyList<VmTerrainGenericDescriptor> descriptors,
        VmTerrainGenericContext ctx,
        List<string> errors)
    {
        while (i < tokens.Count)
        {
            if (tokens[i] == "}") break;
            if (tokens[i] == "{") { i++; continue; }

            var word = tokens[i++];
            var keyword = MatchKeyword(word, keywords);
            if (keyword < 0)
            {
                errors.Add($"Unknown keyword '{word}' in {ctx.OwnerType} {ctx.OwnerName}.");
                return;
            }

            if (keyword >= descriptors.Count)
            {
                errors.Add($"Keyword index {keyword} out of range in {ctx.OwnerType} {ctx.OwnerName}.");
                return;
            }

            var desc = descriptors[keyword];
            switch (desc.Type)
            {
                case VmTerrainGenericType.End:
                    return;
                case VmTerrainGenericType.Sound:
                    ReadString(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Byte:
                    ReadByte(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Class:
                    ReadString(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Splash:
                    ReadString(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Float:
                    ReadDouble(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Double:
                    ReadDouble(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Bool:
                    break;
                case VmTerrainGenericType.Int:
                    ReadInt(tokens, ref i, errors);
                    break;
                case VmTerrainGenericType.Custom:
                    desc.Custom?.Invoke(tokens, ref i, ctx, errors);
                    break;
            }
        }
    }

    public static int MatchKeyword(string word, IReadOnlyList<string> keywords)
    {
        for (var k = 0; k < keywords.Count; k++)
        {
            if (word.Equals(keywords[k], StringComparison.OrdinalIgnoreCase))
                return k;
        }

        return -1;
    }

    internal static string ReadString(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected string."); return ""; }
        return tokens[i++];
    }

    internal static int ReadInt(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !int.TryParse(tokens[i], out var n)) { errors.Add("Expected int."); return 0; }
        i++;
        return n;
    }

    internal static byte ReadByte(IReadOnlyList<string> tokens, ref int i, List<string> errors)
        => (byte)ReadInt(tokens, ref i, errors);

    internal static double ReadDouble(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var f)) { errors.Add("Expected float."); return 0; }
        i++;
        return f;
    }
}
