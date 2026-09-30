namespace HCDE.Playsim;

/// <summary>Native <c>SingleActorFromTID</c> / <c>GetActorIterator</c> subset for ACS inventory.</summary>
internal static class AcsActorTid
{
    public static Actor? SingleFromTid(AuthoritySimulation sim, int tid)
    {
        if (tid == 0) return null;
        return sim.Actors.LastOrDefault(actor => !actor.Destroyed && actor.ThingId == tid);
    }

    public static IEnumerable<Actor> AllFromTid(AuthoritySimulation sim, int tid)
    {
        if (tid == 0) yield break;
        foreach (var actor in sim.Actors)
        {
            if (!actor.Destroyed && actor.ThingId == tid)
                yield return actor;
        }
    }
}
