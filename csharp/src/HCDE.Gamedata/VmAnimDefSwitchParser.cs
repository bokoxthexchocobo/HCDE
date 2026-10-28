namespace HCDE.Gamedata;

/// <summary><c>ProcessSwitchDef</c> from <c>anim_switches.cpp</c>.</summary>
public sealed class VmAnimDefSwitchDefinition
{
    public string Game { get; set; } = "any";
    public string BaseTexture { get; set; } = "";
    public bool QuestPanel { get; set; }
    public List<VmAnimDefSwitchState> OnFrames { get; } = [];
    public List<VmAnimDefSwitchState> OffFrames { get; } = [];
}

public sealed class VmAnimDefSwitchState
{
    public string Texture { get; set; } = "";
    public uint TimeMin { get; set; }
    public uint TimeRnd { get; set; }
    public string Sound { get; set; } = "";
}

public static class VmAnimDefSwitchParser
{
    public static bool TryParseSwitch(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures, List<string> errors)
    {
        var game = "any";
        if (i < tokens.Count)
        {
            var g = tokens[i];
            if (g.Equals("doom", StringComparison.OrdinalIgnoreCase)
                || g.Equals("heretic", StringComparison.OrdinalIgnoreCase)
                || g.Equals("hexen", StringComparison.OrdinalIgnoreCase)
                || g.Equals("strife", StringComparison.OrdinalIgnoreCase)
                || g.Equals("any", StringComparison.OrdinalIgnoreCase))
            {
                game = g.ToLowerInvariant();
                i++;
                if (game == "doom" && i < tokens.Count && int.TryParse(tokens[i], out _))
                    i++;
            }
        }

        if (i >= tokens.Count) { errors.Add("Expected switch texture."); return false; }
        var baseTexture = tokens[i++];
        var def = new VmAnimDefSwitchDefinition { Game = game, BaseTexture = baseTexture };
        while (i < tokens.Count)
        {
            var word = tokens[i];
            if (word.Equals("quest", StringComparison.OrdinalIgnoreCase))
            {
                def.QuestPanel = true;
                i++;
                continue;
            }

            if (word.Equals("on", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (!TryParseSwitchState(tokens, ref i, textures, def.OnFrames, errors)) return false;
                continue;
            }

            if (word.Equals("off", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (!TryParseSwitchState(tokens, ref i, textures, def.OffFrames, errors)) return false;
                continue;
            }

            break;
        }

        if (def.OnFrames.Count == 0)
            errors.Add("Switch requires an on state");
        return errors.Count == 0;
    }

    private static bool TryParseSwitchState(IReadOnlyList<string> tokens, ref int i, IAnimDefTextureCatalog textures,
        List<VmAnimDefSwitchState> frames, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "{") { errors.Add("Expected '{' for switch state."); return false; }
        i++;
        while (i < tokens.Count && tokens[i] != "}")
        {
            if (tokens[i].Equals("sound", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                if (i < tokens.Count) i++;
                continue;
            }

            var tex = tokens[i++];
            if (!VmAnimDefDocumentParser.TryParseTime(tokens, ref i, out var min, out var max, errors)) return false;
            frames.Add(new VmAnimDefSwitchState { Texture = tex, TimeMin = min, TimeRnd = max - min });
        }

        if (i < tokens.Count && tokens[i] == "}") i++;
        return true;
    }
}
