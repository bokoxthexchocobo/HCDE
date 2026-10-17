namespace HCDE.Playsim;

/// <summary>Native <c>CheckActorState</c> subset for managed actor state labels.</summary>
internal static class AcsActorStates
{
    public static bool HasNamedState(Actor actor, string? stateName, bool exact)
        => TryFindNamedState(actor, stateName, exact, out _);

    internal static bool TryFindNamedState(Actor actor, string? stateName, bool exact, out int state)
    {
        state = -1;
        if (actor.Destroyed || string.IsNullOrEmpty(stateName))
            return false;

        var parts = stateName.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return false;
        parts[0] = parts[0].ToUpperInvariant() switch
        {
            "BURN" => "Death.Fire",
            "ICE" => "Death.Ice",
            "DISINTEGRATE" => "Death.Disintegrate",
            "XDEATH" => "Death.Extreme",
            _ => parts[0],
        };
        stateName = string.Join('.', parts);
        var bestLength = -1;
        foreach (var (label, stateIndex) in Labels(actor))
        {
            if (!LabelMatches(stateName, label, exact))
                continue;
            if (stateIndex >= 0 && actor.States.HasState(stateIndex) && label.Length > bestLength)
            {
                state = stateIndex;
                bestLength = label.Length;
            }
        }

        return state >= 0;
    }

    private static IEnumerable<(string Label, int StateIndex)> Labels(Actor actor)
    {
        yield return ("Spawn", actor.SpawnState);
        if (actor.GenericCrushState >= 0) yield return ("GenericCrush", actor.GenericCrushState);
        if (actor.CrushState >= 0) yield return ("Crush", actor.CrushState);
        if (actor.CrashState >= 0) yield return ("Crash", actor.CrashState);
        if (actor.ExtremeCrashState >= 0) yield return ("Crash.Extreme", actor.ExtremeCrashState);
        foreach (var typed in actor.TypedCrashes())
            yield return (typed.Extreme ? "Crash.Extreme." + typed.Type : "Crash." + typed.Type, typed.State);
        if (actor.NullState >= 0) yield return ("Null", actor.NullState);
        if (actor.DieFromSpawnState >= 0) yield return ("DieFromSpawn", actor.DieFromSpawnState);
        if (actor.GenericFreezeDeath >= 0) yield return ("GenericFreezeDeath", actor.GenericFreezeDeath);
        if (actor.HasOnlySpawnLabel) yield break;
        if (actor.ActiveState >= 0) yield return ("Active", actor.ActiveState);
        if (actor.InactiveState >= 0) yield return ("Inactive", actor.InactiveState);
        if (actor.SeeState >= 0)
            yield return ("See", actor.SeeState);
        if (actor.HealState >= 0) yield return ("Heal", actor.HealState);
        yield return ("Pain", actor.PainState);
        if (actor.RaiseState >= 0) yield return ("Raise", actor.RaiseState);
        yield return ("Death", actor.DeathState);
        if (actor.ExtremeDeathState >= 0)
            yield return ("Death.Extreme", actor.ExtremeDeathState);
        yield return ("Corpse", ActorStateMachine.Corpse);
    }

    private static bool LabelMatches(string query, string label, bool exact)
    {
        if (exact)
            return label.Equals(query, StringComparison.OrdinalIgnoreCase);
        return label.Equals(query, StringComparison.OrdinalIgnoreCase)
            || query.StartsWith(label + ".", StringComparison.OrdinalIgnoreCase);
    }
}
