namespace HCDE.Gamedata;

/// <summary><c>FMapInfoParser::ParseLookupName</c> from <c>g_mapinfo.cpp</c> (676–735).</summary>
public static class VmMapInfoParseLookupName
{
    public static bool TryParseLookupToken(string token, out string dest, out bool isLookup)
    {
        if (token.Equals("lookup", StringComparison.OrdinalIgnoreCase))
        {
            dest = "";
            isLookup = true;
            return true;
        }

        if (token.StartsWith('$'))
        {
            dest = token[1..];
            isLookup = true;
            return true;
        }

        dest = token;
        isLookup = false;
        return true;
    }

    public static string JoinMultilineOldFormat(IEnumerable<string> lines)
    {
        var joined = string.Join('\n', lines);
        return joined.Length > 0 ? joined[..^1] : joined;
    }
}
