namespace HCDE.Playsim;

/// <summary>Native <c>SingleActorFromTID</c> / <c>GetActorIterator</c> subset for ACS inventory.</summary>
internal static class AcsActorTid
{
    public static Actor? SingleFromTid(AuthoritySimulation sim, int tid)
    {
        if (tid == 0) return null;
        return sim.Actors.LastOrDefault(actor => !actor.Destroyed && actor.ThingId == tid);
    }

    public static Actor? SelectFromTid(AuthoritySimulation sim, int tid, int index, Actor? activator)
    {
        if (tid == 0)
            return activator is { Destroyed: false } ? activator : null;

        var ordinal = 0;
        foreach (var actor in AllFromTid(sim, tid))
        {
            if (ordinal == index)
                return actor;
            ordinal++;
        }

        return null;
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

    public static bool IsTidUsed(AuthoritySimulation sim, int tid)
    {
        if (tid == 0)
            return false;
        foreach (var actor in sim.Actors)
        {
            if (!actor.Destroyed && actor.ThingId == tid)
                return true;
        }
        return false;
    }

    /// <summary>Native <c>FindUniqueTID</c>: linear search when <paramref name="startTid"/> is non-zero; random probes when zero.</summary>
    public static int FindUniqueTid(AuthoritySimulation sim, int startTid, int searchSpan)
    {
        if (startTid != 0)
        {
            var limit = searchSpan + (startTid - 1);
            for (var tid = startTid; tid <= limit; tid++)
            {
                if (tid != 0 && !IsTidUsed(sim, tid))
                    return tid;
            }
            return 0;
        }

        var probeLimit = searchSpan == 0 ? int.MaxValue : searchSpan;
        for (var i = 0; i < probeLimit; i += 5)
        {
            var tid = (int)(sim.NextUniqueTidRandom() & int.MaxValue);
            tid = FindUniqueTid(sim, tid == 0 ? 1 : tid, 5);
            if (tid != 0)
                return tid;
        }
        return 0;
    }
}
