namespace HCDE.Gamedata;

public sealed record SndInfoMusicVolume(string MusicName, float Volume);
public sealed record SndInfoMusicAlias(string Alias, string Target);
public sealed record SndInfoModPlayer(string MusicName, string Player);
public sealed record SndInfoMidiDevice(string MusicName, string Device, string Args);
public sealed record SndInfoRumbleDef(string Identifier, int Duration, double LowFreq, double HighFreq, double LeftTrig, double RightTrig);
public sealed record SndInfoRumbleMap(string Sound, string RumbleId);

public static class VmSndInfoExtendedDirectives
{
    public static bool TryParseMusicVolume(IReadOnlyList<string> tokens, ref int index, List<SndInfoMusicVolume> volumes, List<string> errors)
    {
        if (index >= tokens.Count) { errors.Add("SNDINFO music volume requires a music name."); return false; }
        var name = Unquote(tokens[index++]);
        if (index >= tokens.Count) { errors.Add("SNDINFO music volume requires a value."); return false; }
        if (!float.TryParse(tokens[index], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var volume))
        {
            errors.Add("Bad value for music volume.");
            return false;
        }

        index++;
        volumes.Add(new SndInfoMusicVolume(name, volume));
        return true;
    }

    public static bool TryParseMusicAlias(IReadOnlyList<string> tokens, ref int index, List<SndInfoMusicAlias> aliases, List<string> errors)
    {
        if (index + 1 >= tokens.Count) { errors.Add("SNDINFO musicalias requires alias and target."); return false; }
        aliases.Add(new SndInfoMusicAlias(Unquote(tokens[index++]), Unquote(tokens[index++])));
        return true;
    }

    public static bool TryParseModPlayer(IReadOnlyList<string> tokens, ref int index, List<SndInfoModPlayer> players, List<string> errors)
    {
        if (index + 1 >= tokens.Count) { errors.Add("SNDINFO modplayer requires music and player."); return false; }
        var music = Unquote(tokens[index++]);
        var player = tokens[index++];
        if (!player.Equals("XMP", StringComparison.OrdinalIgnoreCase) && !player.Equals("libXMP", StringComparison.OrdinalIgnoreCase)
            && !player.Equals("dumb", StringComparison.OrdinalIgnoreCase) && !player.Equals("libdumb", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add($"Unknown Module player {player}");
            return false;
        }

        players.Add(new SndInfoModPlayer(music, player));
        return true;
    }

    public static bool TryParseMidiDevice(IReadOnlyList<string> tokens, ref int index, List<SndInfoMidiDevice> devices, List<string> errors)
    {
        if (index >= tokens.Count) { errors.Add("SNDINFO mididevice requires music name."); return false; }
        var music = Unquote(tokens[index++]);
        if (index >= tokens.Count) { errors.Add("SNDINFO mididevice requires device."); return false; }
        var device = tokens[index++];
        var args = "";
        if (index < tokens.Count && !tokens[index].StartsWith('$'))
            args = Unquote(tokens[index++]);
        devices.Add(new SndInfoMidiDevice(music, device, args));
        return true;
    }

    public static bool TryParseRumbleDef(IReadOnlyList<string> tokens, ref int index, List<SndInfoRumbleDef> defs, List<string> errors)
    {
        if (index >= tokens.Count) { errors.Add("SNDINFO rumbledef requires identifier."); return false; }
        var id = Unquote(tokens[index++]);
        if (index < tokens.Count && !int.TryParse(tokens[index], out _))
        {
            if (index + 1 >= tokens.Count) { errors.Add("SNDINFO rumbledef alias requires target."); return false; }
            index += 2;
            return true;
        }

        if (index + 5 >= tokens.Count) { errors.Add("SNDINFO rumbledef requires timing and frequency fields."); return false; }
        if (!int.TryParse(tokens[index++], out var duration)) return false;
        var low = ReadDouble(tokens, ref index, errors);
        var high = ReadDouble(tokens, ref index, errors);
        var left = ReadDouble(tokens, ref index, errors);
        var right = ReadDouble(tokens, ref index, errors);
        defs.Add(new SndInfoRumbleDef(id, duration, low, high, left, right));
        return errors.Count == 0;
    }

    public static bool TryParseRumbleMap(IReadOnlyList<string> tokens, ref int index, List<SndInfoRumbleMap> maps, List<string> errors)
    {
        if (index + 1 >= tokens.Count) { errors.Add("SNDINFO rumble requires sound and rumble id."); return false; }
        maps.Add(new SndInfoRumbleMap(Unquote(tokens[index++]), Unquote(tokens[index++])));
        return true;
    }

    private static double ReadDouble(IReadOnlyList<string> tokens, ref int index, List<string> errors)
    {
        if (index >= tokens.Count || !double.TryParse(tokens[index], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var value))
        {
            errors.Add("Expected float.");
            return 0;
        }

        index++;
        return value;
    }

    private static string Unquote(string token) => token.StartsWith('"') ? token[1..^1] : token;
}
