namespace HCDE.Playsim;

/// <summary>Native <c>GetNetID</c> / <c>SetActivatorByNetID</c> subset (no live network entity manager).</summary>
internal static class AcsNetId
{
    public const uint WorldNetId = 0;

    public static Actor? ActorFromNetId(AuthoritySimulation sim, uint netId)
    {
        if (netId == WorldNetId)
            return null;
        foreach (var actor in sim.Actors)
        {
            if (!actor.Destroyed && actor.NetworkId == netId)
                return actor;
        }

        return null;
    }

    public static int GetNetId(AuthoritySimulation sim, int tid, int index, Actor? activator, int pointerSelector)
    {
        var actor = AcsActorTid.SelectFromTid(sim, tid, index, activator);
        if (pointerSelector != AcsActorPointer.Default)
            actor = AcsActorPointer.Resolve(sim, actor, pointerSelector);
        return actor is null ? (int)WorldNetId : (int)actor.NetworkId;
    }
}
