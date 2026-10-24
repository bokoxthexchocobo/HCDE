namespace HCDE.Gamedata;

public sealed class VmMapinfoSpawnItem
{
    public string ClassName { get; set; } = "";
    public int LineNum { get; set; }
}

/// <summary><c>ParseSpawnMap</c> / <c>InitClassMap</c> from <c>g_doomedmap.cpp</c>.</summary>
public static class VmDoomEdSpawnMapParser
{
    public static bool TryParseSpawnMap(
        IReadOnlyList<string> tokens,
        ref int i,
        Dictionary<int, VmMapinfoSpawnItem> dest,
        string descriptor,
        List<string> errors)
    {
        var errorCount = 0;
        while (i < tokens.Count && tokens[i] != "}")
        {
            if (!int.TryParse(tokens[i++], out var edNum))
            {
                errors.Add("Number expected.");
                return false;
            }

            if (i >= tokens.Count || tokens[i] != "=") { errors.Add("Expected '='."); return false; }
            i++;
            if (i >= tokens.Count) { errors.Add("Expected class name."); return false; }
            if (dest.ContainsKey(edNum))
            {
                errors.Add($"{descriptor} {edNum} defined more than once");
                errorCount++;
            }
            else if (edNum < 0)
            {
                errors.Add($"{descriptor} must be positive, got {edNum}");
                errorCount++;
            }

            dest[edNum] = new VmMapinfoSpawnItem { ClassName = tokens[i++], LineNum = i };
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        if (errorCount > 0) errors.Add($"{errorCount} errors encountered in {descriptor} definition");
        return errorCount == 0;
    }

    public static void ResolveClassMap(
        IReadOnlyDictionary<int, VmMapinfoSpawnItem> pending,
        Dictionary<int, string> resolved,
        Func<string, bool> validateClass,
        List<string> errors)
    {
        var errorCount = 0;
        foreach (var (key, item) in pending)
        {
            if (!validateClass(item.ClassName))
            {
                errors.Add($"Unknown actor class {item.ClassName} at line {item.LineNum}");
                errorCount++;
                continue;
            }

            resolved[key] = item.ClassName;
        }

        if (errorCount > 0) errors.Add($"{errorCount} unknown actor classes found");
    }
}
