namespace HCDE.Gamedata;

/// <summary>String/path helpers from <c>hcde_edf.cpp</c>.</summary>
public static class HcdeEdfParseUtilities
{
    public static string TrimCopy(string value) => value.Trim();

    public static string LowerCopy(string value) => value.ToLowerInvariant();

    public static string NormalizeResourceName(string value)
    {
        value = value.Replace('\\', '/').TrimStart('/');
        var parts = new List<string>();
        foreach (var part in value.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == "..") { if (parts.Count > 0) parts.RemoveAt(parts.Count - 1); }
            else if (part != ".") parts.Add(part);
        }

        return string.Join('/', parts);
    }

    public static IReadOnlyList<string> TokenizeSimple(string line)
    {
        var tokens = new List<string>();
        var current = "";
        var inQuote = false;
        char quote = '\0';
        foreach (var ch in line)
        {
            if (inQuote)
            {
                if (ch == quote) inQuote = false;
                else current += ch;
                continue;
            }

            if (ch is '"' or '\'') { inQuote = true; quote = ch; continue; }
            if (char.IsWhiteSpace(ch))
            {
                if (current.Length > 0) { tokens.Add(current); current = ""; }
                continue;
            }

            current += ch;
        }

        if (current.Length > 0) tokens.Add(current);
        return tokens;
    }
}
