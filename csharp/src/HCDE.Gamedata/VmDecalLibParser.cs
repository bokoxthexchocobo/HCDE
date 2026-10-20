namespace HCDE.Gamedata;

/// <summary><c>GetDecalID</c>, <c>ReadScale</c>, and decal template keywords from <c>decallib.cpp</c>.</summary>
public sealed class VmDecalTemplate
{
    public double ScaleX { get; set; } = 1;
    public double ScaleY { get; set; } = 1;
    public string PicName { get; set; } = "";
    public string RenderStyle { get; set; } = "Normal";
    public double Alpha { get; set; } = 1;
    public bool FlipX { get; set; }
    public bool FlipY { get; set; }
    public bool RandomFlipX { get; set; }
    public bool RandomFlipY { get; set; }
    public bool FullBright { get; set; }
    public bool Fuzzy { get; set; }
}

public static class VmDecalLibParser
{
    public static bool TryParseDecalId(string token, out ushort id, out bool isNumeric)
    {
        id = 0;
        isNumeric = int.TryParse(token, out var parsed);
        if (!isNumeric) return true;
        if (parsed < 1 || parsed > 65535) return false;
        id = (ushort)parsed;
        return true;
    }

    public static double ReadScale(double value) => Math.Clamp(value, 1 / 256.0, 256.0);

    public static bool TryParseDecalBody(IReadOnlyList<string> tokens, ref int i, VmDecalTemplate template, List<string> errors)
    {
        while (i < tokens.Count)
        {
            if (tokens[i] == "}") break;
            if (tokens[i] == "{") { i++; continue; }
            var key = tokens[i++];
            if (key.Equals("x-scale", StringComparison.OrdinalIgnoreCase))
                template.ScaleX = ReadScale(ReadDouble(tokens, ref i, errors));
            else if (key.Equals("y-scale", StringComparison.OrdinalIgnoreCase))
                template.ScaleY = ReadScale(ReadDouble(tokens, ref i, errors));
            else if (key.Equals("pic", StringComparison.OrdinalIgnoreCase))
                template.PicName = ReadString(tokens, ref i, errors);
            else if (key.Equals("solid", StringComparison.OrdinalIgnoreCase))
                template.RenderStyle = "Normal";
            else if (key.Equals("add", StringComparison.OrdinalIgnoreCase))
            {
                template.Alpha = ReadDouble(tokens, ref i, errors);
                template.RenderStyle = "Add";
            }
            else if (key.Equals("translucent", StringComparison.OrdinalIgnoreCase))
            {
                template.Alpha = ReadDouble(tokens, ref i, errors);
                template.RenderStyle = "Translucent";
            }
            else if (key.Equals("flipx", StringComparison.OrdinalIgnoreCase))
                template.FlipX = true;
            else if (key.Equals("flipy", StringComparison.OrdinalIgnoreCase))
                template.FlipY = true;
            else if (key.Equals("randomflipx", StringComparison.OrdinalIgnoreCase))
                template.RandomFlipX = true;
            else if (key.Equals("randomflipy", StringComparison.OrdinalIgnoreCase))
                template.RandomFlipY = true;
            else if (key.Equals("fullbright", StringComparison.OrdinalIgnoreCase))
                template.FullBright = true;
            else if (key.Equals("fuzzy", StringComparison.OrdinalIgnoreCase))
            {
                template.RenderStyle = "Fuzzy";
                template.Fuzzy = true;
            }
            else
                errors.Add($"Unknown DECALDEF keyword '{key}'.");
        }

        return errors.Count == 0;
    }

    private static string ReadString(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count) { errors.Add("Expected string."); return ""; }
        return tokens[i++];
    }

    private static double ReadDouble(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var f)) { errors.Add("Expected float."); return 0; }
        i++;
        return f;
    }
}
