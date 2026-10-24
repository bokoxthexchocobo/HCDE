namespace HCDE.Gamedata;

/// <summary>Native <c>ModActorFlag</c> / <c>CheckActorFlag</c> subset from <c>thingdef_properties.cpp</c>.</summary>
public static class DecorateActorFlagModifier
{
    public sealed class FlagMutationCounters
    {
        public int Monsters { get; set; }
        public int Items { get; set; }
        public int Secrets { get; set; }
    }

    public static bool TryModify(DecorateActorFlagState state, string flagName, bool set, out string? error) =>
        TryModify(state, flagName, set, counters: null, out error);

    public static bool TryModify(
        DecorateActorFlagState state,
        string flagName,
        bool set,
        FlagMutationCounters? counters,
        out string? error)
    {
        error = null;
        if (!DecorateActorFlagCatalog.TryResolve(flagName, out var entry))
        {
            error = $"'{flagName}' is not a known actor flag.";
            return false;
        }

        if (counters != null)
            ApplyKillItemSecretCounts(state, counters, set: false);

        Apply(state, entry, set);

        if (counters != null)
            ApplyKillItemSecretCounts(state, counters, set: true);

        return true;
    }

    public static bool IsSet(DecorateActorFlagState state, string flagName) =>
        DecorateActorFlagCatalog.TryResolve(flagName, out var entry) && entry.Read!(state);

    public static void ApplyKillItemSecretCounts(DecorateActorFlagState state, FlagMutationCounters counters, bool set)
    {
        if (state.CountsAsKill && state.Health > 0)
            counters.Monsters += set ? 1 : -1;
        if (state.CountItem)
            counters.Items += set ? 1 : -1;
        if (state.CountSecret)
            counters.Secrets += set ? 1 : -1;
    }

    private static void Apply(DecorateActorFlagState state, DecorateActorFlagEntry entry, bool set)
    {
        if (entry.Kind == DecorateActorFlagKind.Deprecated)
        {
            entry.ApplyDeprecated!(state, set);
            return;
        }

        entry.Write!(state, set);
    }
}

public sealed class DecorateActorFlagState
{
    public bool Solid { get; set; }
    public bool Shootable { get; set; } = true;
    public bool CountItem { get; set; }
    public bool CountSecret { get; set; }
    public bool CountsAsKill { get; set; } = true;
    public bool NoBlockmap { get; set; }
    public bool NoSector { get; set; }
    public bool Ambush { get; set; }
    public bool Friendly { get; set; }
    public bool Dormant { get; set; }
    public bool Invisible { get; set; }
    public bool Special { get; set; }
    public bool NoGravity { get; set; }
    public bool Float { get; set; }
    public bool Missile { get; set; }
    public bool NoClip { get; set; }
    public bool WindThrust { get; set; }
    public bool SkullFly { get; set; }
    public int Health { get; set; } = 1000;
}

internal enum DecorateActorFlagKind
{
    Bool,
    Deprecated,
}

internal sealed class DecorateActorFlagEntry
{
    public required string Name { get; init; }
    public DecorateActorFlagKind Kind { get; init; }
    public Func<DecorateActorFlagState, bool>? Read { get; init; }
    public Action<DecorateActorFlagState, bool>? Write { get; init; }
    public Action<DecorateActorFlagState, bool>? ApplyDeprecated { get; init; }
}

internal static class DecorateActorFlagCatalog
{
    private static readonly Dictionary<string, DecorateActorFlagEntry> Flags = Build();

    public static bool TryResolve(string name, out DecorateActorFlagEntry entry)
    {
        var key = name.Trim();
        var dot = key.IndexOf('.');
        if (dot >= 0)
            key = key[..dot];
        return Flags.TryGetValue(key, out entry!);
    }

    public static IReadOnlyCollection<string> Names => Flags.Keys;

    private static Dictionary<string, DecorateActorFlagEntry> Build()
    {
        var map = new Dictionary<string, DecorateActorFlagEntry>(StringComparer.OrdinalIgnoreCase);
        void Add(string name, Func<DecorateActorFlagState, bool> read, Action<DecorateActorFlagState, bool> write) =>
            map[name] = new DecorateActorFlagEntry { Name = name, Kind = DecorateActorFlagKind.Bool, Read = read, Write = write };

        Add("SOLID", s => s.Solid, (s, v) => s.Solid = v);
        Add("SHOOTABLE", s => s.Shootable, (s, v) => s.Shootable = v);
        Add("COUNTITEM", s => s.CountItem, (s, v) => s.CountItem = v);
        Add("COUNTSECRET", s => s.CountSecret, (s, v) => s.CountSecret = v);
        Add("COUNTKILL", s => s.CountsAsKill, (s, v) => s.CountsAsKill = v);
        Add("NOBLOCKMAP", s => s.NoBlockmap, (s, v) => s.NoBlockmap = v);
        Add("NOSECTOR", s => s.NoSector, (s, v) => s.NoSector = v);
        Add("AMBUSH", s => s.Ambush, (s, v) => s.Ambush = v);
        Add("FRIENDLY", s => s.Friendly, (s, v) => s.Friendly = v);
        Add("DORMANT", s => s.Dormant, (s, v) => s.Dormant = v);
        Add("INVISIBLE", s => s.Invisible, (s, v) => s.Invisible = v);
        Add("SPECIAL", s => s.Special, (s, v) => s.Special = v);
        Add("NOGRAVITY", s => s.NoGravity, (s, v) => s.NoGravity = v);
        Add("FLOAT", s => s.Float, (s, v) => s.Float = v);
        Add("MISSILE", s => s.Missile, (s, v) => s.Missile = v);
        Add("NOCLIP", s => s.NoClip, (s, v) => s.NoClip = v);
        Add("WINDTHRUST", s => s.WindThrust, (s, v) => s.WindThrust = v);
        Add("SKULLFLY", s => s.SkullFly, (s, v) => s.SkullFly = v);
        Add("NOBLOCKMONST", s => s.NoBlockmap, (s, v) => s.NoBlockmap = v);

        map["OLDRADIUSDMG"] = new DecorateActorFlagEntry
        {
            Name = "OLDRADIUSDMG",
            Kind = DecorateActorFlagKind.Deprecated,
            ApplyDeprecated = (_, _) => { },
        };

        return map;
    }
}
