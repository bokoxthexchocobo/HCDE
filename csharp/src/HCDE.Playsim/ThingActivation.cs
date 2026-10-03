namespace HCDE.Playsim;

/// <summary>Native Thing_Activate/Deactivate monster fallback without custom state labels.</summary>
public static class ThingActivation
{
    public static bool Execute(AuthoritySimulation sim, Actor? activator, int tid, bool activate)
    {
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets)
        {
            if (!actor.IsMonster || actor.Health <= 0 && !actor.IceCorpse || actor.Dormant == !activate) continue;
            actor.Dormant = !activate;
            actor.States.ForceRemainingTics(activate ? 1 : -1);
        }
        return targets.Length != 0;
    }
}
