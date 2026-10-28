namespace HCDE.Gamedata;

public sealed class HcdeEdfReverbDefinition
{
    public string Name { get; set; } = "";
    public float Room { get; set; } = 0.5f;
    public float Damping { get; set; } = 1f;
    public float WetScale { get; set; } = 0.33333334f;
    public float DryScale { get; set; }
    public float Width { get; set; } = 1f;
    public int Predelay { get; set; }
    public float DecayHfLimit { get; set; } = 1f;
}

public static class HcdeEdfReverbParser
{
    public static bool TryApplyPropertyLine(HcdeEdfReverbDefinition reverb, string line)
    {
        var tokens = HcdeEdfParseUtilities.TokenizeSimple(line);
        if (tokens.Count == 0) return false;
        var key = HcdeEdfParseUtilities.LowerCopy(tokens[0]);
        switch (key)
        {
            case "roomsize" when tokens.Count >= 2:
                reverb.Room = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case "damping" when tokens.Count >= 2:
                reverb.Damping = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case "wetscale" when tokens.Count >= 2:
                reverb.WetScale = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case "dryscale" when tokens.Count >= 2:
                reverb.DryScale = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case "width" when tokens.Count >= 2:
                reverb.Width = float.Parse(tokens[1], System.Globalization.CultureInfo.InvariantCulture);
                return true;
            case "predelay" when tokens.Count >= 2 && int.TryParse(tokens[1], out var pd):
                reverb.Predelay = pd;
                return true;
            case "+equalized":
                reverb.DecayHfLimit = 1f;
                return true;
            case "-equalized":
                reverb.DecayHfLimit = 0f;
                return true;
            default:
                return false;
        }
    }
}
