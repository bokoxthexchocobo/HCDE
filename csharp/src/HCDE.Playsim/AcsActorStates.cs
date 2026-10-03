namespace HCDE.Playsim;

/// <summary>Native <c>CheckActorState</c> subset for managed actor state labels.</summary>
internal static class AcsActorStates
{
    public static bool HasNamedState(Actor actor, string? stateName, bool exact)
    {
        if (actor.Destroyed || string.IsNullOrEmpty(stateName))
            return false;

        foreach (var (label, stateIndex) in Labels(actor))
        {
            if (!LabelMatches(stateName, label, exact))
                continue;
            if (stateIndex >= 0 && actor.States.HasState(stateIndex))
                return true;
        }

        return false;
    }

    private static IEnumerable<(string Label, int StateIndex)> Labels(Actor actor)
    {
        yield return ("Spawn", actor.SpawnState);
        if (actor.ActiveState >= 0) yield return ("Active", actor.ActiveState);
        if (actor.InactiveState >= 0) yield return ("Inactive", actor.InactiveState);
        if (actor.SeeState >= 0)
            yield return ("See", actor.SeeState);
        yield return ("Pain", actor.PainState);
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
            || label.StartsWith(query + ".", StringComparison.OrdinalIgnoreCase);
    }
}
