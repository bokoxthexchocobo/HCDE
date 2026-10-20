namespace HCDE.Gamedata;

/// <summary><c>FIntermissionAction::ParseKey</c> / <c>Parse</c> from <c>intermission_parse.cpp</c>.</summary>
public static class VmIntermissionActionParser
{
    public const int TicRate = 35;

    public static bool TryParseBlock(string body, VmIntermissionAction action, List<string> errors)
    {
        errors.Clear();
        var tokens = VmIntermissionTokenize.Tokenize(body);
        var i = 0;
        while (i < tokens.Count)
        {
            if (tokens[i] == "{") { i++; continue; }
            if (tokens[i] == "}") break;
            var key = tokens[i++];
            if (!TryParseKey(key, ref i, tokens, action, errors))
                errors.Add($"Unknown intermission key '{key}'.");
        }

        return errors.Count == 0;
    }

    public static bool TryParseKey(string key, ref int i, IReadOnlyList<string> tokens, VmIntermissionAction action, List<string> errors)
    {
        if (key.Equals("music", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var music, errors);
            action.Music = music;
            action.MusicOrder = 0;
            if (i < tokens.Count && tokens[i] == ",")
            {
                i++;
                action.MusicOrder = RequireInt(tokens, ref i, errors);
            }

            return errors.Count == 0;
        }

        if (key.Equals("cdmusic", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsInt(tokens, i, out var track, errors);
            action.CdTrack = track;
            action.CdId = 0;
            if (i < tokens.Count && tokens[i] == ",")
            {
                i++;
                action.CdId = RequireInt(tokens, ref i, errors);
            }

            return errors.Count == 0;
        }

        if (key.Equals("Time", StringComparison.OrdinalIgnoreCase))
        {
            i = SkipEquals(tokens, i);
            TryReadTics(tokens, ref i, errors, out var tics);
            action.DurationTics = tics;
            return errors.Count == 0;
        }

        if (key.Equals("Background", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var bg, errors);
            action.Background = bg;
            action.FlatFill = false;
            if (i < tokens.Count && tokens[i] == ",")
            {
                i++;
                action.FlatFill = RequireInt(tokens, ref i, errors) != 0;
            }

            return errors.Count == 0;
        }

        if (key.Equals("Sound", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var snd, errors);
            action.Sound = snd;
            return errors.Count == 0;
        }

        if (key.Equals("Subtitle", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var sub, errors);
            action.Subtitle = sub;
            return errors.Count == 0;
        }

        if (key.Equals("Draw", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var name, errors);
            var x = RequireCommaInt(tokens, ref i, errors);
            var y = RequireCommaInt(tokens, ref i, errors);
            action.Overlays.Add(new VmIntermissionPatch { Name = name, X = x, Y = y });
            return errors.Count == 0;
        }

        if (key.Equals("DrawConditional", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var cond, errors);
            var name = RequireCommaString(tokens, ref i, errors);
            var x = RequireCommaInt(tokens, ref i, errors);
            var y = RequireCommaInt(tokens, ref i, errors);
            action.Overlays.Add(new VmIntermissionPatch { Name = name, X = x, Y = y, Condition = cond });
            return errors.Count == 0;
        }

        if (action is VmIntermissionActionFader fader && key.Equals("FadeType", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsIdentifier(tokens, i, out var id, errors);
            fader.FadeType = id.Equals("FadeOut", StringComparison.OrdinalIgnoreCase)
                ? VmIntermissionFadeType.Out
                : VmIntermissionFadeType.In;
            return errors.Count == 0;
        }

        if (action is VmIntermissionActionWiper wiper && key.Equals("WipeType", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsIdentifier(tokens, i, out var id, errors);
            wiper.WipeType = id switch
            {
                _ when id.Equals("Crossfade", StringComparison.OrdinalIgnoreCase) => VmIntermissionWipeType.Crossfade,
                _ when id.Equals("Melt", StringComparison.OrdinalIgnoreCase) => VmIntermissionWipeType.Melt,
                _ when id.Equals("Burn", StringComparison.OrdinalIgnoreCase) => VmIntermissionWipeType.Burn,
                _ => VmIntermissionWipeType.Default,
            };
            return errors.Count == 0;
        }

        if (action is VmIntermissionActionScroller scroller)
            return TryParseScrollerKey(key, ref i, tokens, scroller, errors);

        if (action is VmIntermissionActionTextscreen text)
            return TryParseTextscreenKey(key, ref i, tokens, text, errors);

        return false;
    }

    private static bool TryParseScrollerKey(string key, ref int i, IReadOnlyList<string> tokens, VmIntermissionActionScroller scroller, List<string> errors)
    {
        if (key.Equals("ScrollDirection", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsIdentifier(tokens, i, out var id, errors);
            scroller.ScrollDir = id switch
            {
                _ when id.Equals("Left", StringComparison.OrdinalIgnoreCase) => VmIntermissionScrollDir.Left,
                _ when id.Equals("Up", StringComparison.OrdinalIgnoreCase) => VmIntermissionScrollDir.Up,
                _ when id.Equals("Down", StringComparison.OrdinalIgnoreCase) => VmIntermissionScrollDir.Down,
                _ => VmIntermissionScrollDir.Right,
            };
            return true;
        }

        if (key.Equals("InitialDelay", StringComparison.OrdinalIgnoreCase))
        {
            i = SkipEquals(tokens, i);
            TryReadTics(tokens, ref i, errors, out var delay);
            scroller.ScrollDelayTics = delay;
            return true;
        }

        if (key.Equals("ScrollTime", StringComparison.OrdinalIgnoreCase))
        {
            i = SkipEquals(tokens, i);
            TryReadTics(tokens, ref i, errors, out var scroll);
            scroller.ScrollTimeTics = scroll;
            return true;
        }

        if (key.Equals("Background2", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var pic, errors);
            scroller.SecondBackground = pic;
            return true;
        }

        return false;
    }

    private static bool TryParseTextscreenKey(string key, ref int i, IReadOnlyList<string> tokens, VmIntermissionActionTextscreen text, List<string> errors)
    {
        if (key.Equals("Position", StringComparison.OrdinalIgnoreCase))
        {
            i = SkipEquals(tokens, i);
            text.TextX = RequireInt(tokens, ref i, errors);
            RequireComma(tokens, ref i, errors);
            text.TextY = RequireInt(tokens, ref i, errors);
            return true;
        }

        if (key.Equals("Text", StringComparison.OrdinalIgnoreCase))
        {
            i = SkipEquals(tokens, i);
            var lines = new List<string>();
            do
            {
                lines.Add(RequireString(tokens, ref i, errors));
            } while (i < tokens.Count && tokens[i] == "," && (tokens[i++] == ","));

            text.Text = lines.Count == 1 && lines[0].StartsWith('$')
                ? lines[0]
                : string.Join('\n', lines);
            return true;
        }

        if (key.Equals("TextColor", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsString(tokens, i, out var color, errors);
            text.TextColor = color;
            return true;
        }

        if (key.Equals("TextDelay", StringComparison.OrdinalIgnoreCase))
        {
            i = SkipEquals(tokens, i);
            TryReadTics(tokens, ref i, errors, out var delay);
            text.TextDelayTics = delay;
            return true;
        }

        if (key.Equals("textspeed", StringComparison.OrdinalIgnoreCase))
        {
            i = RequireEqualsInt(tokens, i, out var speed, errors);
            text.TextSpeed = speed;
            return true;
        }

        return false;
    }

    private static int SkipEquals(IReadOnlyList<string> tokens, int i)
    {
        if (i < tokens.Count && tokens[i] == "=") i++;
        return i;
    }

    private static int RequireEqualsString(IReadOnlyList<string> tokens, int i, out string value, List<string> errors)
    {
        i = SkipEquals(tokens, i);
        value = RequireString(tokens, ref i, errors);
        return i;
    }

    private static int RequireEqualsInt(IReadOnlyList<string> tokens, int i, out int value, List<string> errors)
    {
        i = SkipEquals(tokens, i);
        value = RequireInt(tokens, ref i, errors);
        return i;
    }

    private static int RequireEqualsIdentifier(IReadOnlyList<string> tokens, int i, out string value, List<string> errors)
    {
        i = SkipEquals(tokens, i);
        if (i >= tokens.Count)
        {
            errors.Add("Expected identifier.");
            value = "";
            return i;
        }

        value = tokens[i++];
        return i;
    }

    private static string RequireString(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count)
        {
            errors.Add("Expected string.");
            return "";
        }

        return tokens[i++];
    }

    private static int RequireInt(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !int.TryParse(tokens[i], System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var n))
        {
            errors.Add("Expected integer.");
            return 0;
        }

        i++;
        return n;
    }

    private static bool TryReadTics(IReadOnlyList<string> tokens, ref int i, List<string> errors, out int tics)
    {
        if (i < tokens.Count && tokens[i] == "-")
        {
            i++;
            tics = RequireInt(tokens, ref i, errors);
            return errors.Count == 0;
        }

        if (i < tokens.Count && int.TryParse(tokens[i], System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var direct) && direct < 0)
        {
            i++;
            tics = Math.Abs(direct);
            return true;
        }

        var seconds = RequireFloat(tokens, ref i, errors);
        tics = (int)(seconds * TicRate);
        return errors.Count == 0;
    }

    private static double RequireFloat(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || !double.TryParse(tokens[i], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var f))
        {
            errors.Add("Expected float.");
            return 0;
        }

        i++;
        return f;
    }

    private static void RequireComma(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        if (i >= tokens.Count || tokens[i] != ",") errors.Add("Expected ','.");
        else i++;
    }

    private static int RequireCommaInt(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        RequireComma(tokens, ref i, errors);
        return RequireInt(tokens, ref i, errors);
    }

    private static string RequireCommaString(IReadOnlyList<string> tokens, ref int i, List<string> errors)
    {
        RequireComma(tokens, ref i, errors);
        return RequireString(tokens, ref i, errors);
    }
}
