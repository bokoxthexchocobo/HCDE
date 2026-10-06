namespace HCDE.Playsim;

/// <summary>Native <c>SetActorTeleFog</c> / <c>SwapActorTeleFog</c> subset (stored names only).</summary>
internal static class AcsActorTeleFog
{
    public static int Set(AuthoritySimulation sim, int tid, Actor? activator, string? source, string? dest)
    {
        var count = 0;
        foreach (var actor in Enumerate(sim, tid, activator))
        {
            ActorPropertyActions.SetTeleFog(actor, source, dest);
            count++;
        }

        return count;
    }

    public static int Swap(AuthoritySimulation sim, int tid, Actor? activator)
    {
        var count = 0;
        foreach (var actor in Enumerate(sim, tid, activator))
        {
            ActorPropertyActions.SwapTeleFog(actor);
            count++;
        }

        return count;
    }

    private static IEnumerable<Actor> Enumerate(AuthoritySimulation sim, int tid, Actor? activator)
    {
        if (tid == 0)
        {
            if (activator is { Destroyed: false })
                yield return activator;
            yield break;
        }

        foreach (var actor in AcsActorTid.AllFromTid(sim, tid))
            yield return actor;
    }
}
