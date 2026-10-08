namespace HCDE.MapLoader;

public static class SoundIntegrity
{
    /// <summary>Disables cycle members while retaining definitions that merely lead into a cycle.</summary>
    public static IReadOnlyList<string> Apply(PlayLevel level)
    {
        var broken = new List<string>();
        var owners = level.SoundAliases.Keys.Concat(level.RandomSoundGroups.Keys).Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var owner in owners)
        {
            var pending = new Stack<string>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { owner };
            PushTargets(level, owner, pending);
            while (pending.TryPop(out var current))
            {
                if (current.Equals(owner, StringComparison.OrdinalIgnoreCase)) { broken.Add(owner); break; }
                if (visited.Add(current)) PushTargets(level, current, pending);
            }
        }
        if (broken.Count == 0) return Array.Empty<string>();
        var aliases = new Dictionary<string, string>(level.SoundAliases, StringComparer.OrdinalIgnoreCase);
        var groups = new Dictionary<string, IReadOnlyList<string>>(level.RandomSoundGroups, StringComparer.OrdinalIgnoreCase);
        var sounds = new Dictionary<string, string>(level.SoundDefinitions, StringComparer.OrdinalIgnoreCase);
        foreach (var owner in broken)
        {
            aliases.Remove(owner); groups.Remove(owner); sounds[owner] = "";
        }
        level.SoundAliases = aliases; level.RandomSoundGroups = groups; level.SoundDefinitions = sounds;
        return broken.AsReadOnly();
    }

    private static void PushTargets(PlayLevel level, string sound, Stack<string> pending)
    {
        if (level.SoundAliases.TryGetValue(sound, out var alias)) pending.Push(alias);
        else if (level.RandomSoundGroups.TryGetValue(sound, out var choices))
            foreach (var choice in choices) pending.Push(choice);
    }
}
