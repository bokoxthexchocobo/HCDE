namespace HCDE.MapLoader;

/// <summary>Resolves represented logical sound mappings without loading or playing audio.</summary>
public static class SoundResourceResolver
{
    public static bool TryResolvePlayerSound(PlayLevel level, string sound, string className,
        out string resolved, out string? error, int gender = 0, Func<string, bool>? isAvailable = null)
    {
        if (gender is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(gender));
        resolved = sound; error = null;
        var definitions = level.PlayerSounds;
        if (definitions.Count == 0) return true;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (true)
        {
            var slot = resolved;
            if (!definitions.Any(p => p.Slot.Equals(slot, StringComparison.OrdinalIgnoreCase))) return true;
            if (!visited.Add(slot)) { error = "sound-player-alias-cycle"; return false; }
            HCDE.Gamedata.SndInfoPlayerSound? Find(string name)
            {
                var entries = definitions.Where(p => p.ClassName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (entries.Length == 0) return null;
                var selectedGender = entries.Any(p => p.Gender == gender) ? gender : entries.Min(p => p.Gender);
                bool Matches(HCDE.Gamedata.SndInfoPlayerSound p) => p.Slot.Equals(slot, StringComparison.OrdinalIgnoreCase)
                    && (isAvailable?.Invoke(p.Target) ?? true);
                return entries.FirstOrDefault(p => p.Gender == selectedGender && Matches(p))
                    ?? (selectedGender != 0 ? entries.FirstOrDefault(p => p.Gender == 0 && Matches(p)) : null);
            }
            var definition = Find(className) ?? Find(definitions[0].ClassName);
            if (definition is null) { resolved = ""; return true; }
            resolved = definition.Target;
        }
    }

    /// <summary>Native player lookup considers linked sounds available without picking random replacements.</summary>
    public static bool IsPlayerSoundAvailable(PlayLevel level, string target, IReadOnlySet<string> resources)
    {
        if (TryGetNamedValue(level.SoundAliases, target, out _)
            || TryGetNamedValue(level.RandomSoundGroups, target, out _)
            || level.PlayerSounds.Any(p => p.Slot.Equals(target, StringComparison.OrdinalIgnoreCase))) return true;
        if (!TryGetNamedValue(level.SoundDefinitions, target, out var resource)) return false;
        // DSEMPTY is intentionally silent. Unsupported resource paths must reach the decoder's explicit error.
        return resource.Equals("DSEMPTY", StringComparison.OrdinalIgnoreCase)
            || resource.Length > 8 || resource.Contains('/') || resource.Contains('\\')
            || resources.Contains(resource);
    }

    private static bool TryGetNamedValue<T>(IReadOnlyDictionary<string, T> values, string name, out T value)
    {
        if (values.TryGetValue(name, out value!)) return true;
        foreach (var pair in values)
        {
            if (!pair.Key.Equals(name, StringComparison.OrdinalIgnoreCase)) continue;
            value = pair.Value;
            return true;
        }
        value = default!;
        return false;
    }

    public static HCDE.Gamedata.SndInfoSoundSettings? GetSoundSettings(PlayLevel level, string sound)
        => TryGetNamedValue(level.SoundSettings, sound, out var settings) ? settings : null;

    public static string GetCanonicalSoundName(PlayLevel level, string sound)
    {
        foreach (var name in level.SoundAliases.Keys.Concat(level.SoundDefinitions.Keys)
            .Concat(level.RandomSoundGroups.Keys).Concat(level.SoundSettings.Keys))
            if (name.Equals(sound, StringComparison.OrdinalIgnoreCase)) return name;
        return sound;
    }

    public static bool TryGetSoundLengthMilliseconds(ReadOnlySpan<byte> wad, PlayLevel level, string sound,
        out uint length, out string? error)
        => TryGetSoundLengthMilliseconds(wad, level, sound, new HashSet<string>(StringComparer.OrdinalIgnoreCase), out length, out error);

    private static bool TryGetSoundLengthMilliseconds(ReadOnlySpan<byte> wad, PlayLevel level, string sound,
        HashSet<string> visiting, out uint length, out string? error)
    {
        length = 0;
        error = null;
        sound = GetCanonicalSoundName(level, sound);
        if (!visiting.Add(sound)) { error = "sound-length-cycle"; return false; }
        try
        {
            if (TryGetNamedValue(level.RandomSoundGroups, sound, out var choices))
            {
                foreach (var choice in choices)
                {
                    if (!TryGetSoundLengthMilliseconds(wad, level, choice, visiting, out var duration, out error))
                    { length = 0; return false; }
                    length = Math.Max(length, duration);
                }
                return true;
            }
            if (TryGetNamedValue(level.SoundAliases, sound, out var alias))
                return TryGetSoundLengthMilliseconds(wad, level, alias, visiting, out length, out error);
            if (!TryGetNamedValue(level.SoundDefinitions, sound, out _) && GetSoundSettings(level, sound) is null) return true;
            if (!TryReadDoomSound(wad, level, sound, out var decoded, out error)) return false;
            if (decoded is not null) length = (uint)((ulong)decoded.Samples.Length * 1000 / (uint)decoded.SampleRate);
            return true;
        }
        finally { visiting.Remove(sound); }
    }

    public static bool TryReadSoundCurve(ReadOnlySpan<byte> wad, out byte[] curve, out string? error)
    {
        curve = [];
        if (!TryReadNamedLump(wad, "SNDCURVE", out var bytes, out _, out error)) return false;
        curve = bytes.ToArray();
        return true;
    }

    public static bool TryReadDoomSound(ReadOnlySpan<byte> wad, PlayLevel level, string sound,
        out DoomSound? decoded, out string? error, Func<int>? nextRandom = null)
    {
        decoded = null;
        if (!TryReadWadResource(wad, level, sound, out var bytes, out var found, out error, nextRandom)) return false;
        if (!found || bytes.IsEmpty) return true;
        return DoomSoundDecoder.TryDecode(bytes, out decoded, out error);
    }

    public static bool TryReadWadResource(ReadOnlySpan<byte> wad, PlayLevel level, string sound,
        out ReadOnlySpan<byte> data, out bool found, out string? error, Func<int>? nextRandom = null)
    {
        data = default;
        found = false;
        if (!TryResolve(level, sound, nextRandom, out var resource, out error)) return false;
        if (resource.Length == 0) return true;
        if (resource.Length > 8 || resource.Contains('/') || resource.Contains('\\'))
        {
            error = $"sound-resource-path-unsupported: {resource}";
            return false;
        }
        return TryReadNamedLump(wad, resource, out data, out found, out error);
    }

    private static bool TryReadNamedLump(ReadOnlySpan<byte> wad, string resource,
        out ReadOnlySpan<byte> data, out bool found, out string? error)
    {
        data = default;
        found = false;
        if (!WadArchiveReader.TryReadDirectory(wad, out var entries, out error)) return false;
        for (var index = entries.Length - 1; index >= 0; index--)
        {
            if (!entries[index].Name.Equals(resource, StringComparison.OrdinalIgnoreCase)) continue;
            if (!WadArchiveReader.TryReadLumpData(wad, entries[index], out data, out error)) return false;
            found = true;
            return true;
        }
        return true;
    }

    public static bool TryResolve(PlayLevel level, string sound, out string resource, out string? error)
        => TryResolve(level, sound, null, out resource, out error);

    public static bool TryResolve(PlayLevel level, string sound, Func<int>? nextRandom,
        out string resource, out string? error)
        => TryResolveNamed(level, sound, nextRandom, out resource, out _, out error);

    public static bool TryResolveNamed(PlayLevel level, string sound, Func<int>? nextRandom,
        out string resource, out string resolvedSound, out string? error, Action<string>? visitSound = null,
        string? playerSoundClass = null, int playerGender = 0, Func<string, bool>? playerSoundAvailable = null)
    {
        resource = "";
        resolvedSound = "";
        error = null;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = sound;
        var selectedByRandom = false;
        while (true)
        {
            current = GetCanonicalSoundName(level, current);
            if (!visited.Add(current))
            {
                error = $"sound-alias-cycle: {current}";
                return false;
            }
            // Native PickReplacement skips consecutive random headers in one resolution step.
            if (!selectedByRandom || !TryGetNamedValue(level.RandomSoundGroups, current, out _)) visitSound?.Invoke(current);
            if (level.PlayerSounds.Any(p => p.Slot.Equals(current, StringComparison.OrdinalIgnoreCase)))
            {
                if (!TryResolvePlayerSound(level, current, playerSoundClass ?? "player", out current, out error,
                    playerGender, playerSoundAvailable)) return false;
                if (current.Length == 0) return true;
                selectedByRandom = false;
                continue;
            }
            if (TryGetNamedValue(level.SoundAliases, current, out var target))
            {
                current = target;
                selectedByRandom = false;
                continue;
            }
            if (TryGetNamedValue(level.SoundDefinitions, current, out var value))
            {
                resource = value;
                resolvedSound = current;
                return true;
            }
            if (TryGetNamedValue(level.RandomSoundGroups, current, out var choices))
            {
                if (nextRandom is null)
                {
                    error = $"sound-random-selection-required: {current}";
                    return false;
                }
                if (choices.Count == 0)
                {
                    error = $"sound-random-group-empty: {current}";
                    return false;
                }
                var random = nextRandom();
                if (random < 0)
                {
                    error = "sound-random-value-negative";
                    return false;
                }
                current = choices[random % choices.Count];
                selectedByRandom = true;
                continue;
            }
            if (GetSoundSettings(level, current) is { IsTentative: true })
            {
                resolvedSound = current;
                return true;
            }
            error = $"sound-resource-missing: {current}";
            return false;
        }
    }
}
