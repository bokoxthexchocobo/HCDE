namespace HCDE.Gamedata;

/// <summary><c>FMapInfoParser::ParseEndGame</c> from <c>intermission_parse.cpp</c>.</summary>
public static class VmEndGameParser
{
    private static int _generated;

    public static void ResetGeneratedNames() => _generated = 0;

    public static bool TryParse(string body, out VmEndGameParseResult result, List<string> errors, bool strictUnknown = true)
    {
        result = new VmEndGameParseResult();
        var seq = new VmEndGameSequence();
        var tokens = VmEndGameTokenize.Tokenize(body);
        var i = 0;
        string pic = "", pic2 = "";
        while (i < tokens.Count)
        {
            if (tokens[i] == "{") { i++; continue; }
            if (tokens[i] == "}") { i++; break; }
            var key = tokens[i++];
            if (key.Equals("pic", StringComparison.OrdinalIgnoreCase))
            {
                i = RequireAssignString(tokens, i, out pic, errors);
                seq.PicName = pic;
                seq.EndType = VmEndGameType.Pic;
            }
            else if (key.Equals("hscroll", StringComparison.OrdinalIgnoreCase))
            {
                i = RequireAssignString(tokens, i, out pic, errors);
                seq.PicName = pic;
                RequireComma(tokens, ref i, errors);
                i = RequireString(tokens, i, out pic2, errors);
                seq.PicName2 = pic2;
                if (i < tokens.Count && int.TryParse(tokens[i], out var playEnd))
                {
                    seq.PlayTheEnd = playEnd != 0;
                    i++;
                }
                seq.EndType = VmEndGameType.Bunny;
            }
            else if (key.Equals("vscroll", StringComparison.OrdinalIgnoreCase))
            {
                i = RequireAssignString(tokens, i, out pic, errors);
                seq.PicName = pic;
                RequireComma(tokens, ref i, errors);
                i = RequireString(tokens, i, out pic2, errors);
                seq.PicName2 = pic2;
                seq.EndType = VmEndGameType.Demon;
            }
            else if (key.Equals("cast", StringComparison.OrdinalIgnoreCase))
            {
                seq.EndType = VmEndGameType.Cast;
                if (string.IsNullOrEmpty(seq.PicName))
                    seq.PicName = "$bgcastcall";
            }
            else if (key.Equals("music", StringComparison.OrdinalIgnoreCase))
            {
                i = RequireAssignString(tokens, i, out var music, errors);
                seq.Music = music;
                if (i < tokens.Count && int.TryParse(tokens[i], out var looping))
                {
                    seq.MusicLooping = looping != 0;
                    i++;
                }
            }
            else if (strictUnknown)
            {
                errors.Add($"Unknown property '{key}' in endgame definition.");
                return false;
            }
        }

        if (seq.EndType == VmEndGameType.None)
        {
            errors.Add("Endgame type was not defined");
            return false;
        }

        var desc = new VmIntermissionDescriptor();
        VmIntermissionAction action = seq.EndType switch
        {
            VmEndGameType.Pic => new VmIntermissionAction(),
            VmEndGameType.Bunny => new VmIntermissionActionScroller
            {
                SecondBackground = seq.PicName2,
                ScrollDir = VmIntermissionScrollDir.Left,
                ScrollDelayTics = 230,
                ScrollTimeTics = 640,
                DurationTics = 1130,
            },
            VmEndGameType.Demon => new VmIntermissionActionScroller
            {
                SecondBackground = seq.PicName2,
                ScrollDir = VmIntermissionScrollDir.Up,
                ScrollDelayTics = 70,
                ScrollTimeTics = 600,
            },
            VmEndGameType.Cast => new VmIntermissionAction { DurationTics = 1 },
            _ => new VmIntermissionAction(),
        };

        action.Background = seq.PicName;
        action.Music = seq.Music;
        action.MusicLooping = seq.MusicLooping;
        if (seq.EndType == VmEndGameType.Bunny && seq.PlayTheEnd)
            desc.Link = "TheEnd";
        if (seq.EndType == VmEndGameType.Cast)
            desc.Link = "Doom2Cast";

        desc.Actions.Add(action);
        result.SequenceName = $"@EndSequence_{_generated++}_";
        result.Descriptor = desc;
        return errors.Count == 0;
    }

    private static int RequireAssignString(IReadOnlyList<string> tokens, int i, out string value, List<string> errors)
    {
        if (i < tokens.Count && tokens[i] == "=") i++;
        return RequireString(tokens, i, out value, errors);
    }

    private static int RequireString(IReadOnlyList<string> tokens, int i, out string value, List<string> errors)
    {
        if (i >= tokens.Count)
        {
            errors.Add("Expected string.");
            value = "";
            return i;
        }

        value = tokens[i++];
        return i;
    }

    private static void RequireComma(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != ",") errors.Add("Expected ','.");
        else i++;
    }
}

public sealed class VmEndGameParseResult
{
    public string SequenceName { get; set; } = "";
    public VmIntermissionDescriptor Descriptor { get; set; } = new();
}

internal static class VmEndGameTokenize
{
    public static List<string> Tokenize(string text) => VmLockDefTokenize.Tokenize(text);
}
