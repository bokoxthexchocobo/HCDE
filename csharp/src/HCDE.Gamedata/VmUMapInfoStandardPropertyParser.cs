namespace HCDE.Gamedata;

public sealed class VmUMapEntry
{
    public string LevelName { get; set; } = "";
    public string Author { get; set; } = "";
    public string Label { get; set; } = "";
    public string NextMap { get; set; } = "";
    public string Music { get; set; } = "";
    public string EndPic { get; set; } = "";
    public int NoIntermission { get; set; }
    public int Partime { get; set; }
}

/// <summary><c>ParseStandardProperty</c> subset from <c>umapinfo.cpp</c>.</summary>
public static class VmUMapInfoStandardPropertyParser
{
    public static bool TryApplyProperty(string name, IReadOnlyList<string> tokens, ref int i, VmUMapEntry entry, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "=") { errors.Add("Expected '='."); return false; }
        i++;
        if (name.Equals("levelname", StringComparison.OrdinalIgnoreCase))
        {
            if (i >= tokens.Count) return false;
            entry.LevelName = tokens[i++].Trim('"');
            return true;
        }

        if (name.Equals("author", StringComparison.OrdinalIgnoreCase))
        {
            entry.Author = tokens[i++].Trim('"');
            return true;
        }

        if (name.Equals("label", StringComparison.OrdinalIgnoreCase))
        {
            if (tokens[i].Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                entry.Label = "*";
                return true;
            }

            entry.Label = tokens[i++].Trim('"');
            return true;
        }

        if (name.Equals("next", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("music", StringComparison.OrdinalIgnoreCase))
        {
            var lump = tokens[i++].Trim('"').ToUpperInvariant();
            if (lump.Length > 8) { errors.Add("String too long. Maximum size is 8 characters."); return false; }
            if (name.StartsWith("next", StringComparison.OrdinalIgnoreCase)) entry.NextMap = lump;
            else entry.Music = lump;
            return true;
        }

        if (name.Equals("endcast", StringComparison.OrdinalIgnoreCase))
        {
            var truthy = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase);
            entry.EndPic = truthy ? "$CAST" : "-";
            return true;
        }

        if (name.Equals("nointermission", StringComparison.OrdinalIgnoreCase))
        {
            entry.NoIntermission = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            return true;
        }

        if (name.Equals("partime", StringComparison.OrdinalIgnoreCase))
        {
            entry.Partime = int.Parse(tokens[i++]);
            return true;
        }

        return false;
    }
}
