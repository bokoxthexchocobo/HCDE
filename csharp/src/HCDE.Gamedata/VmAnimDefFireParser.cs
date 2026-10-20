namespace HCDE.Gamedata;

/// <summary><c>ParseFireTexture</c> from <c>animations.cpp</c>.</summary>
public sealed class VmAnimDefFireTexture
{
    public string Name { get; set; } = "";
    public uint DurationMs { get; set; }
    public bool AllowDecals { get; set; }
    public List<string> Colors { get; } = [];
}

public static class VmAnimDefFireParser
{
    public static bool TryParseFire(IReadOnlyList<string> tokens, ref int i, VmAnimDefFireTexture fire, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected fire texture name."); return false; }
        fire.Name = tokens[i++];
        if (i >= tokens.Count || !tokens[i].Equals("tics", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Expected tics duration.");
            return false;
        }

        i++;
        if (i >= tokens.Count || !double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var tics))
        {
            errors.Add("Expected tics value.");
            return false;
        }

        i++;
        fire.DurationMs = (uint)(tics * 1000 / 35.0);
        while (i < tokens.Count)
        {
            if (tokens[i].Equals("allowdecals", StringComparison.OrdinalIgnoreCase)) { fire.AllowDecals = true; i++; continue; }
            if (tokens[i].Equals("color", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i + 3 >= tokens.Count) { errors.Add("Expected RGBA color."); return false; }
                fire.Colors.Add(string.Join(',', tokens.Skip(i).Take(4)));
                i += 4;
                if (fire.Colors.Count > 256) { errors.Add("Too many colors specified!"); return false; }
                continue;
            }

            if (tokens[i].Equals("palette", StringComparison.OrdinalIgnoreCase))
            {
                i += 2;
                continue;
            }

            break;
        }

        return errors.Count == 0;
    }
}
