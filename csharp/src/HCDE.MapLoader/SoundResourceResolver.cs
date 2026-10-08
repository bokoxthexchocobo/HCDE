namespace HCDE.MapLoader;

/// <summary>Resolves represented logical sound mappings without loading or playing audio.</summary>
public static class SoundResourceResolver
{
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
        out string resource, out string resolvedSound, out string? error, Action<string>? visitSound = null)
    {
        resource = "";
        resolvedSound = "";
        error = null;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var current = sound;
        var selectedByRandom = false;
        while (true)
        {
            if (!visited.Add(current))
            {
                error = $"sound-alias-cycle: {current}";
                return false;
            }
            // Native PickReplacement skips consecutive random headers in one resolution step.
            if (!selectedByRandom || !level.RandomSoundGroups.ContainsKey(current)) visitSound?.Invoke(current);
            if (level.SoundAliases.TryGetValue(current, out var target))
            {
                current = target;
                selectedByRandom = false;
                continue;
            }
            if (level.SoundDefinitions.TryGetValue(current, out var value))
            {
                resource = value;
                resolvedSound = current;
                return true;
            }
            if (level.RandomSoundGroups.TryGetValue(current, out var choices))
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
            if (level.SoundSettings.TryGetValue(current, out var settings) && settings.IsTentative)
            {
                resolvedSound = current;
                return true;
            }
            error = $"sound-resource-missing: {current}";
            return false;
        }
    }
}
