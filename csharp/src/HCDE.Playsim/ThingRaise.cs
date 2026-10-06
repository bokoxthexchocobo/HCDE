namespace HCDE.Playsim;

internal static class ThingRaise
{
    internal static bool? Execute(AuthoritySimulation sim, int special, Actor? activator, int tid, int flags)
    {
        if (special != 17) return null;
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : AcsActorTid.AllFromTid(sim, tid).ToArray();
        var raised = false;
        foreach (var corpse in targets)
        {
            // A null line-special activator cannot transfer friendliness.
            raised |= ActorRaiseActions.RaiseActor(activator ?? corpse, corpse,
                activator is null ? flags & ~1 : flags);
        }
        return raised;
    }
}
