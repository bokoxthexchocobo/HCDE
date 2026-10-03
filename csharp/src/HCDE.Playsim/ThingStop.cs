namespace HCDE.Playsim;

/// <summary>Native Thing_Stop actor velocity action.</summary>
public static class ThingStop
{
    public static bool? ExecuteSpecial(AuthoritySimulation sim, int special, Actor? activator, int tid)
    {
        if (special != 19) return null;
        var targets = tid == 0
            ? activator is { Destroyed: false } ? new[] { activator } : Array.Empty<Actor>()
            : sim.Actors.Where(actor => !actor.Destroyed && actor.ThingId == tid).ToArray();
        foreach (var actor in targets)
            actor.VelocityX = actor.VelocityY = actor.VelocityZ = default;
        return targets.Length != 0;
    }
}