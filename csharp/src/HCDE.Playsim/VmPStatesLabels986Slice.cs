namespace HCDE.Playsim;

/// <summary>
/// Label destroy, special death scan, state-name parsing, and multi-name <c>FindState</c> from <c>p_states.cpp</c> (**151–300**).
/// </summary>
public static class VmPStatesLabels986Slice
{
    public const string DeathLabel = "Death";
    public const string UnknownStateName = "<unknown>";

    public static int DestroyNestedLabelChildren(
        int numLabels,
        Func<int, object?> getChildLabels,
        Action<object?>? destroyChildTree,
        Action<object?>? freeChildStorage)
    {
        if (numLabels <= 0 || getChildLabels is null)
            return 0;

        var tornDown = 0;
        for (var i = 0; i < numLabels; i++)
        {
            var children = getChildLabels(i);
            if (children is null)
                continue;
            destroyChildTree?.Invoke(children);
            freeChildStorage?.Invoke(children);
            tornDown++;
        }

        return tornDown;
    }

    public static bool ActorHasSpecialDeathStates(
        object? stateLabels,
        Func<object?, object?>? findDeathLabel,
        Func<object?, int>? childLabelCount,
        Func<object?, int, object?>? childStateAt)
    {
        if (stateLabels is null || findDeathLabel is null || childLabelCount is null || childStateAt is null)
            return false;

        var deathLabel = findDeathLabel(stateLabels);
        if (deathLabel is null)
            return false;

        var count = childLabelCount(deathLabel);
        for (var i = 0; i < count; i++)
        {
            if (childStateAt(deathLabel, i) is not null)
                return true;
        }

        return false;
    }

    public static bool TryRemapLegacyStateAlias(string firstToken, out string mappedFirst, out string? mappedSecond)
    {
        mappedSecond = null;
        switch (firstToken)
        {
            case "Burn":
                mappedFirst = DeathLabel;
                mappedSecond = "Fire";
                return true;
            case "Ice":
                mappedFirst = DeathLabel;
                mappedSecond = "Ice";
                return true;
            case "Disintegrate":
                mappedFirst = DeathLabel;
                mappedSecond = "Disintegrate";
                return true;
            case "XDeath":
                mappedFirst = DeathLabel;
                mappedSecond = "Extreme";
                return true;
            default:
                mappedFirst = firstToken;
                return true;
        }
    }

    public static int BuildStateNameParts(ReadOnlySpan<string> dottedParts, IList<string> output)
    {
        output.Clear();
        if (dottedParts.Length == 0)
            return 0;

        TryRemapLegacyStateAlias(dottedParts[0], out var first, out var second);
        output.Add(first);
        if (second is not null)
            output.Add(second);

        for (var i = 1; i < dottedParts.Length; i++)
            output.Add(dottedParts[i]);

        return output.Count;
    }

    public static bool ExactFindStateMisses(bool exact, int matchedNames, int requestedNames) =>
        exact && matchedNames < requestedNames;

    public static object? FindBestStateForNames(
        object? rootLabels,
        ReadOnlySpan<string> names,
        bool exact,
        Func<object?, string, (object? childLabels, object? state)>? resolveLabel)
    {
        if (rootLabels is null || resolveLabel is null || names.Length == 0)
            return null;

        object? labels = rootLabels;
        object? best = null;
        var matched = 0;

        while (labels is not null && matched < names.Length)
        {
            var (childLabels, state) = resolveLabel(labels, names[matched]);
            if (childLabels is null && state is null)
                break;

            matched++;
            labels = childLabels;
            if (state is not null)
                best = state;
        }

        if (ExactFindStateMisses(exact, matched, names.Length))
            return null;

        return best;
    }
}
