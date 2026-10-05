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
            if (!ActorRaise.HasRaiseState(corpse)) continue;
            corpse.VelocityX = corpse.VelocityY = default;
            if ((flags & 2) == 0 && !ActorRaise.CheckPosition(sim, corpse)) continue;
            ActorRaise.ReviveSupported(corpse);
            raised = true;
        }
        return raised;
    }
}
