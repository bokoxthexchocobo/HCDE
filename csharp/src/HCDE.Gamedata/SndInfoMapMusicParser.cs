using System.Globalization;
using System.Text.RegularExpressions;

namespace HCDE.Gamedata;

public enum SoundRolloffType { Doom, Linear, Log, Custom }
public sealed record SoundRolloff(SoundRolloffType Type = SoundRolloffType.Doom, float MinDistance = 0, float MaxDistance = 0);

public sealed record SndInfoSoundSettings(float Volume = 1, float Attenuation = 1, int NearLimit = 2, float LimitRange = 65536,
    bool Singular = false, int PitchMask = 0, float DefPitch = 0, float DefPitchMax = 0, bool IsTentative = false,
    SoundRolloff? Rolloff = null);

public sealed record SndInfoData(IReadOnlyDictionary<int, string> MapMusic, IReadOnlyDictionary<string, string> Sounds)
{
    public int CurrentPitchMask { get; init; }
    public SoundRolloff? GlobalRolloff { get; init; }
    public IReadOnlyDictionary<string, SndInfoSoundSettings> Settings { get; init; } = new Dictionary<string, SndInfoSoundSettings>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, IReadOnlyList<string>> RandomGroups { get; init; } = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlyDictionary<string, string> Aliases { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}

/// <summary>Reads SNDINFO map music and logical sound mappings; unsupported directives fail explicitly.</summary>
public static class SndInfoMapMusicParser
{
    public static bool TryParse(string text, out IReadOnlyDictionary<int, string> music, out string? error,
        IReadOnlyDictionary<int, string>? previous = null)
    {
        var success = TryParseData(text, out var data, out error,
            previous is null ? null : new SndInfoData(previous, new Dictionary<string, string>()));
        music = data.MapMusic;
        return success;
    }

    public static bool TryParseData(string text, out SndInfoData data, out string? error, SndInfoData? previous = null)
    {
        var result = previous is null ? new Dictionary<int, string>() : new Dictionary<int, string>(previous.MapMusic);
        var sounds = previous is null ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(previous.Sounds, StringComparer.OrdinalIgnoreCase);
        var aliases = new Dictionary<string, string>(previous?.Aliases ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
        var settings = new Dictionary<string, SndInfoSoundSettings>(previous?.Settings ?? new Dictionary<string, SndInfoSoundSettings>(), StringComparer.OrdinalIgnoreCase);
        var currentPitchMask = previous?.CurrentPitchMask ?? 0;
        var globalRolloff = previous?.GlobalRolloff;
        var groups = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in previous?.RandomGroups ?? new Dictionary<string, IReadOnlyList<string>>()) groups[pair.Key] = Array.AsReadOnly(pair.Value.ToArray());
        data = new(new Dictionary<int, string>(), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
        error = null;
        var tokens = new List<string>();
        var position = 0;
        foreach (Match match in Regex.Matches(text, "\\s+|//[^\\r\\n]*|/\\*[\\s\\S]*?\\*/|\"[^\"]*\"|[^\\s\"/={}]+(?:/(?![/*])[^\\s\"/={}]+)*|[={} ]"))
        {
            if (match.Index != position) { error = "Invalid SNDINFO token or unterminated comment/string."; return false; }
            position = match.Index + match.Length;
            var token = match.Value;
            if (char.IsWhiteSpace(token[0]) || token.StartsWith("//") || token.StartsWith("/*")) continue;
            tokens.Add(token);
        }
        if (position != text.Length) { error = "Invalid SNDINFO token or unterminated comment/string."; return false; }
        bool? assignments = null;
        for (var index = 0; index < tokens.Count;)
        {
            if (tokens[index].Equals("$rolloff", StringComparison.OrdinalIgnoreCase))
            {
                index++;
                if (index >= tokens.Count || tokens[index].StartsWith('$') || tokens[index] is "=" or "{" or "}")
                { error = "SNDINFO rolloff is missing a name."; return false; }
                var name = Unquote(tokens[index++]);
                var prior = name == "*" ? globalRolloff : settings.GetValueOrDefault(name)?.Rolloff;
                var type = prior?.Type ?? SoundRolloffType.Doom;
                if (index < tokens.Count && !float.TryParse(tokens[index], NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                {
                    var token = tokens[index++];
                    if (token.Equals("linear", StringComparison.OrdinalIgnoreCase)) type = SoundRolloffType.Linear;
                    else if (token.Equals("log", StringComparison.OrdinalIgnoreCase)) type = SoundRolloffType.Log;
                    else if (token.Equals("custom", StringComparison.OrdinalIgnoreCase)) type = SoundRolloffType.Custom;
                    else { error = "Unknown SNDINFO rolloff type."; return false; }
                }
                if (index + 1 >= tokens.Count
                    || !float.TryParse(tokens[index++], NumberStyles.Float, CultureInfo.InvariantCulture, out var minimum) || !float.IsFinite(minimum)
                    || !float.TryParse(tokens[index++], NumberStyles.Float, CultureInfo.InvariantCulture, out var maximum) || !float.IsFinite(maximum))
                { error = "Invalid SNDINFO rolloff distances."; return false; }
                var rolloff = new SoundRolloff(type, minimum, maximum);
                if (name == "*") globalRolloff = rolloff;
                else settings[name] = (settings.GetValueOrDefault(name) ?? new SndInfoSoundSettings(IsTentative: true)) with { Rolloff = rolloff };
                continue;
            }
            if (tokens[index].Equals("$pitchshiftrange", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 1 >= tokens.Count || !int.TryParse(tokens[index + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var shift))
                { error = "Invalid SNDINFO global pitch shift."; return false; }
                currentPitchMask = (1 << Math.Clamp(shift, 0, 7)) - 1;
                index += 2;
                continue;
            }
            if (tokens[index].Equals("$singular", StringComparison.OrdinalIgnoreCase)
                || tokens[index].Equals("$pitchshift", StringComparison.OrdinalIgnoreCase)
                || tokens[index].Equals("$pitchset", StringComparison.OrdinalIgnoreCase))
            {
                var directive = tokens[index++];
                if (index >= tokens.Count || tokens[index].StartsWith('$') || tokens[index] is "=" or "{" or "}")
                { error = "SNDINFO sound setting is missing a name."; return false; }
                var name = Unquote(tokens[index++]);
                var updated = settings.GetValueOrDefault(name) ?? new SndInfoSoundSettings(IsTentative: true);
                if (directive.Equals("$singular", StringComparison.OrdinalIgnoreCase)) updated = updated with { Singular = true };
                else if (directive.Equals("$pitchshift", StringComparison.OrdinalIgnoreCase))
                {
                    if (index >= tokens.Count || !int.TryParse(tokens[index++], NumberStyles.Integer, CultureInfo.InvariantCulture, out var shift))
                    { error = "Invalid SNDINFO pitch shift."; return false; }
                    updated = updated with { PitchMask = (1 << Math.Clamp(shift, 0, 7)) - 1 };
                }
                else
                {
                    if (index >= tokens.Count || !float.TryParse(tokens[index++], NumberStyles.Float, CultureInfo.InvariantCulture, out var pitch) || !float.IsFinite(pitch))
                    { error = "Invalid SNDINFO pitch value."; return false; }
                    float maximum = 0;
                    if (index < tokens.Count && float.TryParse(tokens[index], NumberStyles.Float, CultureInfo.InvariantCulture, out maximum))
                    {
                        if (!float.IsFinite(maximum)) { error = "Invalid SNDINFO pitch maximum."; return false; }
                        index++;
                    }
                    updated = updated with { DefPitch = pitch, DefPitchMax = maximum };
                }
                settings[name] = updated;
                continue;
            }
            if (tokens[index].Equals("$limit", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 2 >= tokens.Count || tokens[index + 1].StartsWith('$') || tokens[index + 1] is "=" or "{" or "}"
                    || !int.TryParse(tokens[index + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var limit))
                { error = "Invalid SNDINFO sound limit."; return false; }
                var name = Unquote(tokens[index + 1]);
                var prior = settings.GetValueOrDefault(name) ?? new SndInfoSoundSettings(IsTentative: true);
                var updated = prior with { NearLimit = Math.Clamp(limit, 0, 255) };
                index += 3;
                if (index < tokens.Count && double.TryParse(tokens[index], NumberStyles.Float, CultureInfo.InvariantCulture, out var distance))
                {
                    var squared = (float)(distance * distance);
                    if (!double.IsFinite(distance) || !float.IsFinite(squared))
                    { error = "Invalid SNDINFO sound limit distance."; return false; }
                    updated = updated with { LimitRange = squared };
                    index++;
                }
                settings[name] = updated;
                continue;
            }
            if (tokens[index].Equals("$volume", StringComparison.OrdinalIgnoreCase)
                || tokens[index].Equals("$attenuation", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 2 >= tokens.Count || tokens[index + 1].StartsWith('$') || tokens[index + 1] is "=" or "{" or "}"
                    || !float.TryParse(tokens[index + 2], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || !float.IsFinite(value))
                { error = "Invalid SNDINFO sound volume/attenuation setting."; return false; }
                var name = Unquote(tokens[index + 1]);
                var prior = settings.GetValueOrDefault(name) ?? new SndInfoSoundSettings(IsTentative: true);
                settings[name] = tokens[index].Equals("$volume", StringComparison.OrdinalIgnoreCase)
                    ? prior with { Volume = value } : prior with { Attenuation = value };
                index += 3;
                continue;
            }
            if (tokens[index].Equals("$random", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 2 >= tokens.Count || tokens[index + 1].StartsWith('$') || tokens[index + 1] is "=" or "{" or "}" || tokens[index + 2] != "{")
                { error = "SNDINFO $random requires a name and choice block."; return false; }
                var owner = Unquote(tokens[index + 1]);
                settings.TryAdd(owner, new SndInfoSoundSettings(PitchMask: currentPitchMask));
                index += 3;
                var choices = new List<string>();
                while (index < tokens.Count && tokens[index] != "}")
                {
                    if (tokens[index].StartsWith('$') || tokens[index] is "{" or "=")
                    { error = "Invalid SNDINFO random choice."; return false; }
                    var choice = Unquote(tokens[index++]);
                    settings.TryAdd(choice, new SndInfoSoundSettings(IsTentative: true));
                    choices.Add(choice);
                }
                if (index >= tokens.Count) { error = "Unterminated SNDINFO random group."; return false; }
                index++;
                sounds.Remove(owner); aliases.Remove(owner); groups.Remove(owner);
                ResetInheritedLimit(settings, owner);
                if (choices.Count == 0) sounds[owner] = "";
                else if (choices.Count == 1) aliases[owner] = choices[0];
                else groups[owner] = choices.AsReadOnly();
                if (choices.Count != 0) settings[owner] = (settings.GetValueOrDefault(owner) ?? new SndInfoSoundSettings()) with { NearLimit = -1 };
                continue;
            }
            if (tokens[index].Equals("$alias", StringComparison.OrdinalIgnoreCase))
            {
                if (index + 2 >= tokens.Count || tokens[index + 1].StartsWith('$') || tokens[index + 2].StartsWith('$')
                    || tokens[index + 1] == "=" || tokens[index + 2] == "=")
                { error = "SNDINFO $alias requires a name and target."; return false; }
                var name = Unquote(tokens[index + 1]);
                settings.TryAdd(name, new SndInfoSoundSettings(PitchMask: currentPitchMask));
                ResetInheritedLimit(settings, name);
                settings[name] = (settings.GetValueOrDefault(name) ?? new SndInfoSoundSettings()) with { NearLimit = -1 };
                aliases[name] = Unquote(tokens[index + 2]);
                settings.TryAdd(aliases[name], new SndInfoSoundSettings(IsTentative: true));
                sounds.Remove(name);
                groups.Remove(name);
                index += 3;
                continue;
            }
            if (!tokens[index].Equals("$map", StringComparison.OrdinalIgnoreCase))
            {
                var name = tokens[index++];
                if (name.StartsWith('$')) { error = $"Unsupported SNDINFO directive {name}."; return false; }
                var hasAssignment = index < tokens.Count && tokens[index] == "=";
                assignments ??= hasAssignment;
                if (assignments.Value && !hasAssignment) { error = "SNDINFO sound mapping requires an assignment."; return false; }
                if (hasAssignment && assignments.Value) index++;
                if (index >= tokens.Count || tokens[index].StartsWith('$') || tokens[index] == "=")
                { error = "SNDINFO sound mapping is missing a resource."; return false; }
                name = Unquote(name);
                settings.TryAdd(name, new SndInfoSoundSettings(PitchMask: currentPitchMask));
                ResetInheritedLimit(settings, name);
                sounds[name] = Unquote(tokens[index++]);
                aliases.Remove(name);
                groups.Remove(name);
                continue;
            }
            if (index + 2 >= tokens.Count || !int.TryParse(tokens[index + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            { error = "SNDINFO $map requires a map number and music resource."; return false; }
            var resource = tokens[index + 2];
            if (resource.StartsWith('$')) { error = "SNDINFO $map is missing a music resource."; return false; }
            if (resource.StartsWith('"')) resource = resource[1..^1];
            if (number != 0) result[number] = resource;
            index += 3;
        }
        data = new(result, sounds) { Aliases = aliases, RandomGroups = groups, Settings = settings, CurrentPitchMask = currentPitchMask, GlobalRolloff = globalRolloff };
        return true;
    }

    private static string Unquote(string token) => token.StartsWith('"') ? token[1..^1] : token;

    private static void ResetInheritedLimit(Dictionary<string, SndInfoSoundSettings> settings, string name)
    {
        if (settings.TryGetValue(name, out var prior))
            settings[name] = prior.NearLimit == -1
                ? prior with { NearLimit = 2, LimitRange = 65536, IsTentative = false }
                : prior with { IsTentative = false };
    }
}
