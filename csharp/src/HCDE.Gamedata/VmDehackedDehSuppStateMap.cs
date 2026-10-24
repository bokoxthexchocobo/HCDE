namespace HCDE.Gamedata;

public sealed class VmDehackedStateMapEntry
{
    public string ActorType { get; init; } = "";
    public string StateLabel { get; init; } = "";
    public int StateSpan { get; init; }
    public bool OwnerIsPickup { get; init; }
    public int FirstDehIndex { get; init; }
}

/// <summary><c>LoadDehSupp</c> <c>StateMap</c> from <c>d_dehacked.cpp</c> (3598–3652).</summary>
public static class VmDehackedDehSuppStateMap
{
    public const uint MfSpecial = 0x00000080;

    public static bool ShouldAppendToStateMap(int existingEntryCount) => existingEntryCount == 0;

    public static bool IsPickupActor(uint actorFlags) => (actorFlags & MfSpecial) != 0;

    public static bool TryValidateActorType(string typeName, Func<string, bool> typeExists, Func<string, bool> isDescendantOfActor, out string? error)
    {
        error = null;
        if (!typeExists(typeName))
        {
            error = $"Can't find type {typeName}";
            return false;
        }

        if (!isDescendantOfActor(typeName))
        {
            error = $"{typeName} is not an actor";
            return false;
        }

        return true;
    }

    public static bool TryValidateStateRange(
        string actorType,
        string stateLabel,
        int stateSpan,
        bool appendMode,
        bool stateResolved,
        Func<string, int, bool> ownsStateThroughSpan,
        out string? error)
    {
        error = null;
        if (!appendMode)
            return true;
        if (!stateResolved)
        {
            error = $"Invalid state '{stateLabel}' in '{actorType}'";
            return false;
        }

        if (stateSpan < 1 || !ownsStateThroughSpan(actorType, stateSpan))
        {
            error = $"Invalid state range in '{actorType}'";
            return false;
        }

        return true;
    }

    public static Dictionary<int, int> BuildDehExtStates(IReadOnlyList<VmDehackedStateMapEntry> entries)
    {
        var map = new Dictionary<int, int>();
        var dehCount = 0;
        foreach (var entry in entries)
        {
            for (var i = 0; i < entry.StateSpan; i++)
                map[dehCount++] = dehCount - 1;
        }

        return map;
    }

    public static bool TryParseStateMapSection(
        IReadOnlyList<(string ActorType, string StateLabel, int StateSpan, uint ActorFlags)> rows,
        IReadOnlyList<VmDehackedStateMapEntry> existingStateMap,
        Func<string, bool> typeExists,
        Func<string, bool> isDescendantOfActor,
        Func<string, string, bool> tryResolveState,
        Func<string, int, bool> ownsStateThroughSpan,
        out List<VmDehackedStateMapEntry> stateMap,
        out Dictionary<int, int> dehExtStates,
        out string? error)
    {
        stateMap = existingStateMap.Count == 0 ? [] : [.. existingStateMap];
        dehExtStates = BuildDehExtStates(stateMap);
        error = null;
        var append = ShouldAppendToStateMap(existingStateMap.Count);
        var dehCount = dehExtStates.Count;

        foreach (var (actorType, stateLabel, stateSpan, actorFlags) in rows)
        {
            if (!TryValidateActorType(actorType, typeExists, isDescendantOfActor, out error))
                return false;

            var stateResolved = tryResolveState(actorType, stateLabel);
            if (!TryValidateStateRange(actorType, stateLabel, stateSpan, append, stateResolved, ownsStateThroughSpan, out error))
                return false;

            if (append)
            {
                stateMap.Add(new VmDehackedStateMapEntry
                {
                    ActorType = actorType,
                    StateLabel = stateLabel,
                    StateSpan = stateSpan,
                    OwnerIsPickup = IsPickupActor(actorFlags),
                    FirstDehIndex = dehCount
                });
                dehCount += stateSpan;
            }
        }

        dehExtStates = BuildDehExtStates(stateMap);
        return true;
    }
}
