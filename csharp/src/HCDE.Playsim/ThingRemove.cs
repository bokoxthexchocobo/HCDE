namespace HCDE.Playsim;

/// <summary>Native Thing_Remove action for managed world actors.</summary>
public static class ThingRemove
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator, int tid)
    {
        if (special != 132) return null;
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets)
            if (actor is not PlayerPawn) actor.Destroy();
        return true;
    }
}