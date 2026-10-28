namespace HCDE.Gamedata;

/// <summary>EMAPINFO line helpers from <c>hcde_emapinfo.cpp</c>.</summary>
public static class VmEMapInfoLineParser
{
    public static string TrimCopy(string value) => value.Trim();

    public static string StripOuterQuotes(string value)
    {
        if (value.Length >= 2)
        {
            var first = value[0];
            var last = value[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                return value[1..^1];
        }

        return value;
    }

    public static string StripLineComment(string line)
    {
        var inQuote = false;
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '"') inQuote = !inQuote;
            if (inQuote) continue;
            if (line[i] == '#' || line[i] == ';') return line[..i].TrimEnd();
            if (line[i] == '/' && i + 1 < line.Length && line[i + 1] == '/') return line[..i].TrimEnd();
        }

        return line;
    }

    public static bool SplitPropertyLine(string line, out string key, out string value)
    {
        key = "";
        value = "";
        var eq = line.IndexOf('=');
        if (eq >= 0)
        {
            key = TrimCopy(line[..eq]);
            value = TrimCopy(line[(eq + 1)..]);
        }
        else
        {
            var split = 0;
            while (split < line.Length && !char.IsWhiteSpace(line[split])) split++;
            key = TrimCopy(line[..split]);
            value = TrimCopy(split < line.Length ? line[(split + 1)..] : "");
        }

        key = key.ToLowerInvariant();
        value = StripOuterQuotes(TrimCopy(value));
        return key.Length > 0;
    }

    public static IReadOnlyList<string> SplitValueList(string value)
    {
        var result = new List<string>();
        var current = "";
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch) || ch is ',' or '|' or '+')
            {
                if (current.Length > 0) { result.Add(current); current = ""; }
            }
            else current += ch;
        }

        if (current.Length > 0) result.Add(current);
        return result;
    }

    public static string NormalizeResourceName(string value)
    {
        value = value.Replace('\\', '/');
        while (value.StartsWith('/')) value = value[1..];
        return value;
    }

    public static bool HasPathOrExtension(string value) =>
        value.Contains('/') || value.Contains('.');

    public static bool TryParseBool(string value, out bool result)
    {
        if (value.Length == 0) { result = true; return true; }
        var lowered = value.ToLowerInvariant();
        if (lowered is "true" or "yes" or "on" or "1") { result = true; return true; }
        if (lowered is "false" or "no" or "off" or "0") { result = false; return true; }
        result = false;
        return false;
    }

    public static bool TryParseInt(string value, out int result) =>
        int.TryParse(value, out result);
}
