namespace HCDE.Gamedata;

public sealed class VmUMapEntry
{
    public string LevelName { get; set; } = "";
    public string Author { get; set; } = "";
    public string Label { get; set; } = "";
    public string NextMap { get; set; } = "";
    public string Music { get; set; } = "";
    public string NextSecret { get; set; } = "";
    public string SkyTexture { get; set; } = "";
    public string LevelPic { get; set; } = "";
    public string ExitPic { get; set; } = "";
    public string EnterPic { get; set; } = "";
    public string ExitAnim { get; set; } = "";
    public string EnterAnim { get; set; } = "";
    public string EndPic { get; set; } = "";
    public int NoIntermission { get; set; }
    public int Partime { get; set; }
    public string InterText { get; set; } = "";
    public string InterTextSecret { get; set; } = "";
    public string InterBackdrop { get; set; } = "FLOOR4_8";
    public string InterMusic { get; set; } = "";
    public VmUMapPlayerActionOption Jumping { get; set; } = VmUMapPlayerActionOption.Unset;
    public VmUMapPlayerActionOption Crouching { get; set; } = VmUMapPlayerActionOption.Unset;
    public VmUMapPlayerActionOption FreeAim { get; set; } = VmUMapPlayerActionOption.Unset;
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
            name.Equals("nextsecret", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("music", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("skytexture", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("levelpic", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("endpic", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("exitpic", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("enterpic", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("exitanim", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("enteranim", StringComparison.OrdinalIgnoreCase))
        {
            var lump = tokens[i++].Trim('"').ToUpperInvariant();
            if (lump.Length > 8) { errors.Add("String too long. Maximum size is 8 characters."); return false; }
            switch (name.ToLowerInvariant())
            {
                case "next": entry.NextMap = lump; break;
                case "nextsecret": entry.NextSecret = lump; break;
                case "music": entry.Music = lump; break;
                case "skytexture": entry.SkyTexture = lump; break;
                case "levelpic": entry.LevelPic = lump; break;
                case "endpic": entry.EndPic = lump; break;
                case "exitpic": entry.ExitPic = lump; break;
                case "enterpic": entry.EnterPic = lump; break;
                case "exitanim": entry.ExitAnim = lump; break;
                case "enteranim": entry.EnterAnim = lump; break;
            }

            return true;
        }

        if (name.Equals("endbunny", StringComparison.OrdinalIgnoreCase))
        {
            entry.EndPic = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase) ? "$BUNNY" : "-";
            return true;
        }

        if (name.Equals("endgame", StringComparison.OrdinalIgnoreCase))
        {
            entry.EndPic = tokens[i++].Equals("true", StringComparison.OrdinalIgnoreCase) ? "!" : "-";
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

        if (name.Equals("intertext", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("intertextsecret", StringComparison.OrdinalIgnoreCase))
        {
            if (!VmUMapInfoTokenParser.TryParseMultiString(tokens, ref i, out var text, errors)) return false;
            if (text.Length == 0) return false;
            if (name.StartsWith("intertextsecret", StringComparison.OrdinalIgnoreCase)) entry.InterTextSecret = text;
            else entry.InterText = text;
            return true;
        }

        if (name.Equals("interbackdrop", StringComparison.OrdinalIgnoreCase))
        {
            Span<char> buf = stackalloc char[9];
            if (!VmUMapInfoTokenParser.TryParseLumpName(tokens[i++].Trim('"'), buf, errors)) return false;
            entry.InterBackdrop = new string(buf).TrimEnd('\0');
            return true;
        }

        if (name.Equals("intermusic", StringComparison.OrdinalIgnoreCase))
        {
            Span<char> buf = stackalloc char[9];
            if (!VmUMapInfoTokenParser.TryParseLumpName(tokens[i++].Trim('"'), buf, errors)) return false;
            entry.InterMusic = new string(buf).TrimEnd('\0');
            return true;
        }

        if (name.Equals("jumping", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("crouching", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("freeaim", StringComparison.OrdinalIgnoreCase))
        {
            if (!VmUMapInfoTokenParser.TryParsePlayerActionOption(tokens[i++], out var opt)) return false;
            if (name.Equals("jumping", StringComparison.OrdinalIgnoreCase)) entry.Jumping = opt;
            else if (name.Equals("crouching", StringComparison.OrdinalIgnoreCase)) entry.Crouching = opt;
            else entry.FreeAim = opt;
            return true;
        }

        return false;
    }

    public static bool TryApplyEpisodeProperty(
        IReadOnlyList<string> tokens,
        ref int i,
        VmUMapEntry entry,
        string mapName,
        IList<VmUMapEpisode> episodes,
        ref int id24LevelNum,
        List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != "=") { errors.Add("Expected '='."); return false; }
        i++;
        if (!VmUMapInfoTokenParser.TryParseMultiString(tokens, ref i, out var episode, errors)) return false;
        if (episode.Length == 0) return false;
        return VmUMapInfoEpisodeParser.TryApplyEpisode(episode, mapName, episodes, ref id24LevelNum);
    }
}
