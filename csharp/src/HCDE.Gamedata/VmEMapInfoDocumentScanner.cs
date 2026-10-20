namespace HCDE.Gamedata;

/// <summary><c>HCDE_ParseEMapInfo</c> scan loop from <c>hcde_emapinfo.cpp</c> (825–922).</summary>
public static class VmEMapInfoDocumentScanner
{
    public static bool TryParseSectionHeader(string line, out string mapName)
    {
        mapName = "";
        var trimmed = line.Trim();
        if (trimmed.Length < 2 || trimmed[0] != '[') return false;
        var close = trimmed.IndexOf(']');
        if (close < 1) return false;
        mapName = trimmed[1..close].Trim().ToUpperInvariant();
        return mapName.Length > 0;
    }

    public static IEnumerable<string> EnumerateLines(string text)
    {
        var start = 0;
        while (start <= text.Length)
        {
            var end = text.IndexOf('\n', start);
            if (end < 0) end = text.Length;
            var line = text[start..end];
            if (line.EndsWith('\r')) line = line[..^1];
            yield return line;
            if (end >= text.Length) break;
            start = end + 1;
        }
    }
}
