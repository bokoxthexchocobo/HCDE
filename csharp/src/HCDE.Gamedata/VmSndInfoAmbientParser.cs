namespace HCDE.Gamedata;

/// <summary><c>SI_Ambient</c> parsing from <c>s_advsound.cpp</c>.</summary>
public sealed record SndInfoAmbientSound(
    int Index,
    string LogicalSound,
    string Placement,
    double? Attenuation,
    string Mode,
    double PeriodMinSeconds,
    double PeriodMaxSeconds,
    float Volume);

public static class VmSndInfoAmbientParser
{
    public static bool TryParse(IReadOnlyList<string> tokens, ref int index, List<SndInfoAmbientSound> ambients, List<string> errors)
    {
        if (index >= tokens.Count || !int.TryParse(tokens[index], out var slot))
        {
            errors.Add("SNDINFO ambient requires a numeric slot.");
            return false;
        }

        index++;
        if (index >= tokens.Count) { errors.Add("SNDINFO ambient requires a logical sound."); return false; }
        var logical = Unquote(tokens[index++]);
        var placement = "world";
        double? attenuation = 0;
        if (index >= tokens.Count) { errors.Add("SNDINFO ambient requires placement."); return false; }
        var placeToken = tokens[index++];
        if (placeToken.Equals("point", StringComparison.OrdinalIgnoreCase))
        {
            placement = "point";
            attenuation = 1;
            if (index < tokens.Count && double.TryParse(tokens[index], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var att))
            {
                attenuation = att > 0 ? att : 1;
                index++;
            }
        }
        else if (placeToken.Equals("surround", StringComparison.OrdinalIgnoreCase))
        {
            placement = "surround";
            attenuation = -1;
        }
        else if (placeToken.Equals("world", StringComparison.OrdinalIgnoreCase))
            placement = "world";
        else
        {
            errors.Add("Unknown SNDINFO ambient placement.");
            return false;
        }

        if (index >= tokens.Count) { errors.Add("SNDINFO ambient requires a mode."); return false; }
        var mode = tokens[index++];
        var periodMin = 0.0;
        var periodMax = 0.0;
        if (mode.Equals("random", StringComparison.OrdinalIgnoreCase))
        {
            periodMin = ReadDouble(tokens, ref index, errors);
            periodMax = ReadDouble(tokens, ref index, errors);
        }
        else if (mode.Equals("periodic", StringComparison.OrdinalIgnoreCase))
            periodMin = ReadDouble(tokens, ref index, errors);

        if (index >= tokens.Count) { errors.Add("SNDINFO ambient requires volume."); return false; }
        var volume = (float)Math.Clamp(ReadDouble(tokens, ref index, errors), 0, 1);
        ambients.Add(new SndInfoAmbientSound(slot, logical, placement, attenuation, mode, periodMin, periodMax, volume));
        return errors.Count == 0;
    }

    private static double ReadDouble(IReadOnlyList<string> tokens, ref int index, List<string> errors)
    {
        if (index >= tokens.Count) { errors.Add("Expected float."); return 0; }
        if (!double.TryParse(tokens[index], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            errors.Add("Expected float.");
            return 0;
        }

        index++;
        return value;
    }

    private static string Unquote(string token) => token.StartsWith('"') ? token[1..^1] : token;
}
